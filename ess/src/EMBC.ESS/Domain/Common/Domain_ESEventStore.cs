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
            var evt = e.Event.Data.DeserializeEvent(Type.GetType(e.Event.EventType));
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
                OperationOptions = new EventStoreClientOperationOptions { ThrowOnAppendFailure = true, TimeoutAfter = TimeSpan.FromSeconds(5) },
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
            replayer.ReplayInto(handlersToReplay).Wait();
            var subscriptionManager = sp.GetRequiredService<SubscriptionManager>();
            subscriptionManager.SubscribePublisher().Wait();
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
            await foreach (var handler in handlersToReplay.ToAsyncEnumerable())
            {
                await ReplayInto(conn, handler);
            }
        }

        private async Task ReplayInto(EventStoreClient conn, Type handlerType)
        {
            var handler = serviceProvider.GetRequiredService(handlerType);
            var handlers = handler.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(mi => mi.GetParameters().Length == 1)
                     .Select(mi => new { mi = mi, et = mi.GetParameters()[0].ParameterType })
                     .Where(m => typeof(Event).IsAssignableFrom(m.et))
                    .ToDictionary(m => m.et);

            await conn.ReadAllAsync(Direction.Forwards, Position.Start)
                .ForEachAsync(evt =>
                {
                    if (evt.Event.EventType.StartsWith("$")) return;
                    var e = evt.ToDomainEvent();
                    if (!handlers.ContainsKey(e.GetType())) return;
                    handlers[e.GetType()].mi.Invoke(handler, new[] { e });
                });
        }
    }

    public class SubscriptionManager
    {
        private readonly IServiceProvider serviceProvider;

        public SubscriptionManager(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task SubscribePublisher()
        {
            var publisher = serviceProvider.GetRequiredService<IEventPublisher>();
            var conn = serviceProvider.GetRequiredService<EventStoreClient>();
            var logger = serviceProvider.GetRequiredService<ILogger<SubscriptionManager>>();
            logger.LogDebug("Subscribing event publisher to event store");
            try
            {
                var sub = await conn.SubscribeToAllAsync(Position.End,
                    eventAppeared: async (sub, e, _) =>
                    {
                        await publisher.PublishAsync(e.ToDomainEvent());
                    },
                    filterOptions: new SubscriptionFilterOptions(EventTypeFilter.ExcludeSystemEvents()),
                    subscriptionDropped: (sub, reason, e) =>
                    {
                        logger.LogError("Event store dropped subscription {0} of event publisher: {1}. Resubscribing...", sub.SubscriptionId, reason);
                        SubscribePublisher().Wait();
                    });
                logger.LogInformation("Subscribed event publisher to event store, subscription id {0}", sub.SubscriptionId);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to subscribe event publisher to evet store: {0}", e.Message);
                Thread.Sleep(TimeSpan.FromSeconds(5).Milliseconds);
                await SubscribePublisher();
            }
        }
    }
}
