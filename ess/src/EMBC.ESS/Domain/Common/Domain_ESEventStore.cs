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
using EventTypeFilter = EventStore.Client.EventTypeFilter;

namespace EMBC.ESS.Domain.Common
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
            var revision = expectedVersion == 0 ? StreamRevision.None : new StreamRevision(expectedVersion);
            var serializedEvents = events.Select(e => e.FromDomainEvent()).ToArray();
            await conn.AppendToStreamAsync(eventStreamId, revision, serializedEvents);
        }
    }

    public static class EventStoreEx
    {
        private static Event DeserializeEvent(this ReadOnlyMemory<byte> data, Type eventType) => (Event)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data.Span.ToArray()), eventType);

        private static ReadOnlyMemory<byte> SerializeEvent(this Event evt) => new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(evt)));

        public static Event ToDomainEvent(this ResolvedEvent e)
        {
            var evt = e.Event.Data.DeserializeEvent(Type.GetType(e.Event.EventType, true));
            evt.Version = e.Event.EventNumber.ToUInt64();
            return evt;
        }

        public static EventData FromDomainEvent(this Event evt)
        {
            return new EventData(Uuid.NewUuid(), evt.GetType().FullName, evt.SerializeEvent());
        }
    }

    public static class ESEvenStoreConfigEx
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
            services.AddSingleton<EventReplayer>();
            services.AddSingleton<SubscriptionManager>();
            return services;
        }

        public static IApplicationBuilder InitializeESEventStore(this IApplicationBuilder builder, params Type[] handlersToReplay)
        {
            var sp = builder.ApplicationServices;
            var replayer = sp.GetRequiredService<EventReplayer>();
            replayer.ReplayInto(handlersToReplay).GetAwaiter().GetResult();
            var subscriptionManager = sp.GetRequiredService<SubscriptionManager>();
            subscriptionManager.SubscribePublisher(Position.End).Wait();
            return builder;
        }
    }

    public class EventReplayer
    {
        private readonly IServiceProvider serviceProvider;

        public EventReplayer(IServiceProvider serviceProvider)
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
        private ILogger<SubscriptionManager> logger;
        private readonly IEventPublisher publisher;
        private readonly EventStoreClient conn;
        private StreamSubscription sub;

        public SubscriptionManager(IServiceProvider serviceProvider)
        {
            logger = serviceProvider.GetRequiredService<ILogger<SubscriptionManager>>();
            publisher = serviceProvider.GetRequiredService<IEventPublisher>();
            conn = serviceProvider.GetRequiredService<EventStoreClient>();
        }

        public async Task SubscribePublisher(Position lastKnown)
        {
            logger.LogDebug("Subscribing event publisher to EventStore");
            var lastKnownPosition = lastKnown;
            try
            {
                sub = await conn.SubscribeToAllAsync(lastKnown,
                    eventAppeared: async (sub, e, _) =>
                    {
                        await publisher.PublishAsync(e.ToDomainEvent());
                        lastKnownPosition = e.Event.Position;
                    },
                    filterOptions: new SubscriptionFilterOptions(EventTypeFilter.ExcludeSystemEvents()),
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
}
