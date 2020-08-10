using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EventStore.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

    public static class ESEvenStoreConfigigurationEx
    {
        public static IServiceCollection AddESEventStore(this IServiceCollection services)
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
            services.AddSingleton<EventsReplayer>();
            services.AddSingleton<SubscriptionManager>();
            services.AddTransient<IProvideSequenceNumbers, SequenceNumberProvider>();
            return services;
        }

        public static IApplicationBuilder InitializeESEventStore(this IApplicationBuilder builder, Type[] handlersToReplay = null, Type[] readModelBuilders = null)
        {
            var sp = builder.ApplicationServices;
            var replayer = sp.GetRequiredService<EventsReplayer>();
            if (handlersToReplay != null) replayer.ReplayInto(handlersToReplay).GetAwaiter().GetResult();
            if (readModelBuilders != null)
            {
                foreach (var readModel in readModelBuilders)
                {
                    var instance = builder.ApplicationServices.GetRequiredService(readModel);
                    var mi = readModel.GetMethod("BuildAsync");
                    mi.Invoke(instance, new object[0]);
                }
            }
            var subscriptionManager = sp.GetRequiredService<SubscriptionManager>();
            subscriptionManager.SubscribePublisher(Position.End).Wait();
            return builder;
        }
    }

    public class EventsReplayer
    {
        private readonly IServiceProvider serviceProvider;

        public EventsReplayer(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task ReplayInto(params Type[] handlersToReplay)
        {
            var conn = serviceProvider.GetRequiredService<EventStoreClient>();
            await foreach (var handlerType in handlersToReplay.ToAsyncEnumerable())
            {
                var handler = serviceProvider.GetRequiredService(handlerType);
                await ReplayInto(conn, handler);
            }
        }

        private async Task ReplayInto(EventStoreClient conn, object handler)
        {
            var handlers = handler.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(mi => mi.GetParameters().Length == 1)
                     .Select(mi => new { mi = mi, et = mi.GetParameters()[0].ParameterType })
                     .Where(m => typeof(Event).IsAssignableFrom(m.et))
                    .ToDictionary(m => m.et);

            await conn.ReadAllAsync(Direction.Forwards, Position.Start)
                .ForEachAsync(evt =>
                {
                    if (evt.Event.EventType.StartsWith("$") || evt.OriginalStreamId.StartsWith("$")) return;
                    var e = evt.ToDomainEvent();
                    if (!handlers.ContainsKey(e.GetType())) return;
                    handlers[e.GetType()].mi.Invoke(handler, new[] { e });
                });
        }
    }

    public class SubscriptionManager
    {
        private readonly ILogger<SubscriptionManager> logger;
        private readonly IEventPublisher publisher;
        private readonly EventStoreClient conn;
        private StreamSubscription sub;

        public SubscriptionManager(IServiceProvider serviceProvider)
        {
            logger = serviceProvider.GetRequiredService<ILogger<SubscriptionManager>>();
            publisher = serviceProvider.GetRequiredService<IEventPublisher>();
            conn = serviceProvider.GetRequiredService<EventStoreClient>();
        }

        public async Task SubscribePublisher(Position lastKnownPosition)
        {
            logger.LogDebug("Subscribing event publisher to EventStore");
            try
            {
                sub = await conn.SubscribeToAllAsync(lastKnownPosition,
                    eventAppeared: async (sub, e, _) =>
                    {
                        await publisher.PublishAsync(e.ToDomainEvent());
                        lastKnownPosition = e.Event.Position;
                    },
                    filterOptions: new SubscriptionFilterOptions(EventStoreClientNS.EventTypeFilter.ExcludeSystemEvents()),
                    subscriptionDropped: (sub, reason, e) =>
                    {
                        logger.LogError("EventStore dropped subscription {0} of event publisher: {1}. Resubscribing from position {2}...", sub.SubscriptionId, reason, lastKnownPosition);
                        sub.Dispose();
                        SubscribePublisher(lastKnownPosition).Wait();
                    });
                logger.LogInformation("Subscribed event publisher to event store, subscription id {0}", sub.SubscriptionId);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to subscribe event publisher to EventStore: {0}", e.Message);
                if (sub != null) sub.Dispose();
                await Task.Delay(TimeSpan.FromSeconds(5).Milliseconds).ContinueWith(_ => SubscribePublisher(lastKnownPosition));
            }
        }
    }

    public class ESReadModelRepository<TItem> : IReadModelRepository<TItem> where TItem : AggregateRoot
    {
        private readonly IRepository<TItem> repository;
        private readonly EventStoreClient conn;
        private static readonly string streamPrefix = typeof(TItem).FullName;

        public ESReadModelRepository(IRepository<TItem> repository, EventStoreClient conn)
        {
            this.repository = repository;
            this.conn = conn;
        }

        public IAsyncEnumerable<TItem> GetAsync(Func<TItem, bool> filter = null)
        {
            var query = conn
                .ReadStreamAsync(Direction.Forwards, GetCategoryStreamName(), StreamPosition.Start)
                .SelectAwait(async e =>
                {
                    var originalStreamId = Encoding.UTF8.GetString(e.Event.Data.ToArray());
                    return await GetByIdAsync(GetIdFromStreamName(originalStreamId));
                });

            if (filter != null) { query = query.Where(filter); }

            return query;
        }

        public async Task<TItem> GetByIdAsync(string id)
        {
            return await repository.GetByIdAsync(id);
        }

        private static string GetCategoryStreamName() => $"$category-{streamPrefix}";

        private static string GetIdFromStreamName(string streamName) => streamName.Substring(streamPrefix.Length + 1);

        private static string GetStreamName(string id) => $"{typeof(TItem).FullName}-{id}";
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
                var lastSequenceEvent = (SequenceEvent)await eventStore.GetEventsAsync(streamName).SingleOrDefaultAsync();
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
            }

            await eventStore.SaveEventsAsync(streamName, new[] { new SequenceEvent { Version = nextVersion, LastSequenceValue = nextSequence } }, expectedVersion);

            return nextSequence;
        }
    }
}
