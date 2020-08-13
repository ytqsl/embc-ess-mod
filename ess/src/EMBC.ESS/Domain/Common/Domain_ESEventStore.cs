using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

using EventStoreClientNS = EventStore.Client;

namespace EMBC.ESS.Domain.Common.EventStore
{
    public class ESEventStore : IEventStore
    {
        private readonly EventStoreClient conn;

        public ESEventStore(EventStoreClient conn)
        {
            this.conn = conn ?? throw new ArgumentNullException(nameof(conn));
        }

        public IAsyncEnumerable<Event> GetEventsAsync(string eventStreamId)
        {
            Trace.Assert(!string.IsNullOrEmpty(eventStreamId));
            return conn.ReadStreamAsync(Direction.Forwards, eventStreamId, StreamPosition.Start).Select(e => e.ToDomainEvent());
        }

        public async Task SaveEventsAsync(string eventStreamId, IEnumerable<Event> events, ulong expectedVersion)
        {
            Trace.Assert(!string.IsNullOrEmpty(eventStreamId));
            Trace.Assert(events != null);
            var revision = expectedVersion == ulong.MaxValue ? StreamRevision.None : new StreamRevision(expectedVersion);
            var serializedEvents = events.Select(e => e.FromDomainEvent()).ToArray();
            await conn.AppendToStreamAsync(eventStreamId, revision, serializedEvents);
        }
    }

    public static class EventStoreSerializationEx
    {
        public static T Deserialize<T>(this ReadOnlyMemory<byte> data, Type type) => (T)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data.Span.ToArray()), type);

        public static ReadOnlyMemory<byte> Serialize<T>(this T obj) => new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj)));

        public static Event ToDomainEvent(this ResolvedEvent e)
        {
            var evt = e.Event.Data.Deserialize<Event>(Type.GetType(e.Event.EventType, true));
            evt.Version = e.Event.EventNumber.ToUInt64();
            return evt;
        }

        public static EventData FromDomainEvent(this Event evt)
        {
            return new EventData(Uuid.NewUuid(), evt.GetType().FullName, evt.Serialize());
        }
    }

    public static class ESEvenStoreConfigurationEx
    {
        public static IServiceCollection AddESEventStore(this IServiceCollection services)
        {
            var readModelBuilders = Assembly.GetExecutingAssembly().GetExportedTypes().Where(t => t.Name.EndsWith("Builder")).ToArray();
            return services.AddESEventStore(readModelBuilders);
        }

        public static IServiceCollection AddESEventStore(this IServiceCollection services, Type[] readModels)
        {
            var settings = new EventStoreClientSettings
            {
                OperationOptions = new EventStoreClientOperationOptions { ThrowOnAppendFailure = true },
                CreateHttpMessageHandler = () => new SocketsHttpHandler
                {
                    SslOptions = { RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true }
                },
                DefaultCredentials = new UserCredentials("admin", "changeit"),
                ConnectivitySettings = new EventStoreClientConnectivitySettings()
                {
                    Address = new Uri("https://localhost:2113")
                },
                ConnectionName = "EMBC.ESS",
            };
            var conn = new EventStoreClient(settings);
            services.AddSingleton(conn);
            services.AddSingleton<IEventStore, ESEventStore>();
            services.AddTransient<IProvideSequenceNumbers, SequenceNumberProvider>();
            services.AddHostedService(sp => new ProjectorHost(sp, readModels));
            foreach (var readModel in readModels)
            {
                services.AddTransient(readModel);
            }
            return services;
        }
    }

    public class ProjectorHost : IHostedService
    {
        private readonly EventsProjector projector;

        public ProjectorHost(IServiceProvider serviceProvider, Type[] readModelBuilders = null)
        {
            projector = new EventsProjector(serviceProvider, readModelBuilders);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await projector.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            projector.Dispose();
        }
    }

    public class EventsProjector : IDisposable
    {
        private StreamSubscription subscription;
        private IServiceProvider serviceProvider;
        private readonly Type[] readModelBuilderTypes;
        private bool disposedValue;

        public EventsProjector(IServiceProvider serviceProvider, params Type[] readModelBuilderTypes)
        {
            this.serviceProvider = serviceProvider;
            this.readModelBuilderTypes = readModelBuilderTypes;
        }

        public async Task StartAsync() => await Project();

        private async Task Project()
        {
            var conn = serviceProvider.GetRequiredService<EventStoreClient>();
            foreach (var handlerType in readModelBuilderTypes)
            {
                var eventPublisher = new EventStorePublisher(serviceProvider.GetRequiredService(handlerType));
                await ProjectInto(conn, eventPublisher);
            }
        }

        private async Task ProjectInto(EventStoreClient conn, IEventPublisher eventPublisher)
        {
            Position checkpoint = Position.Start;
            subscription = await conn.SubscribeToAllAsync(
               start: checkpoint,
               resolveLinkTos: true,
               eventAppeared: async (sub, e, _) =>
               {
                   await eventPublisher.PublishAsync(e.ToDomainEvent());
                   checkpoint = e.OriginalPosition.Value;
               },
               subscriptionDropped: (sub, reason, _) =>
               {
               },
               filterOptions: new SubscriptionFilterOptions(EventStoreClientNS.EventTypeFilter.ExcludeSystemEvents()));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (subscription != null) subscription.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class EventStorePublisher : IEventPublisher
    {
        private readonly IDictionary<Type, MethodInfo[]> eventHandlersMap;
        private readonly object handlerHost;

        public EventStorePublisher(object handlerHost)
        {
            this.handlerHost = handlerHost;
            var eventHandlers = this.handlerHost.GetType().GetMethods()
                  .Where(mi => mi.Name.StartsWith("Handle") && mi.GetParameters().Length == 1 && typeof(Event).IsAssignableFrom(mi.GetParameters()[0].ParameterType));

            eventHandlersMap = eventHandlers.GroupBy(mi => mi.GetParameters()[0].ParameterType).ToDictionary(g => g.Key, mi => mi.ToArray());
        }

        public async Task PublishAsync<T>(T evt) where T : Event
        {
            var eventType = evt.GetType();
            if (!eventHandlersMap.TryGetValue(eventType, out var handlers)) { return; }
            foreach (var handler in handlers)
            {
                if (handler.ReturnType == typeof(Task) || handler.ReturnType == typeof(ValueTask))
                {
                    var result = (Task)handler.Invoke(handlerHost, new[] { evt });
                    await result;
                }
                else
                {
                    handler.Invoke(handlerHost, new[] { evt });
                }
            }
        }
    }

    public class SequenceNumberProvider : IProvideSequenceNumbers
    {
        private class SequenceEvent : Event
        {
            public ulong LastSequenceValue { get; set; }
        }

        private readonly IEventStore eventStore;

        public SequenceNumberProvider(IEventStore eventStore)
        {
            this.eventStore = eventStore;
        }

        public async Task<ulong> NextAsync<TItem>() where TItem : AggregateRoot
        {
            var streamName = $"sequence-{typeof(TItem).FullName}";
            ulong nextSequence;
            ulong nextVersion;
            ulong expectedVersion;
            try
            {
                var lastSequenceEvent = (SequenceEvent)await eventStore.GetEventsAsync(streamName).LastOrDefaultAsync();
                nextSequence = lastSequenceEvent.LastSequenceValue + 1;
                expectedVersion = lastSequenceEvent.Version;
                nextVersion = expectedVersion + 1;
            }
            catch (EventStoreClientNS.StreamNotFoundException)
            {
                //new sequence will be created
                nextSequence = 1;
                expectedVersion = ulong.MaxValue;
                nextVersion = 0;
                //TODO: need to set stream metadata to keep only 1 event
            }

            await eventStore.SaveEventsAsync(streamName, new[] { new SequenceEvent { Version = nextVersion, LastSequenceValue = nextSequence } }, expectedVersion);

            return nextSequence;
        }
    }
}
