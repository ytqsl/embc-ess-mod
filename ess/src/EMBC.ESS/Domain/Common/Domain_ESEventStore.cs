using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Embedded;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace EMBC.ESS.Domain.Common
{
    public class ESEventStore : IEventStore
    {
        private readonly IEventStoreConnection conn;

        public ESEventStore(IEventStoreConnection conn, IEventPublisher eventPublisher)
        {
            if (eventPublisher is null) { throw new ArgumentNullException(nameof(eventPublisher)); }
            this.conn = conn ?? throw new ArgumentNullException(nameof(conn));

            RegisterSubscribers(conn, eventPublisher);
        }

        private static void RegisterSubscribers(IEventStoreConnection conn, IEventPublisher eventPublisher)
        {
            conn.FilteredSubscribeToAllAsync(true, Filter.ExcludeSystemEvents, async (sub, evt) =>
            {
                var evtDeserialized = evt.Event.Data.DeserializeEvent(Type.GetType(evt.Event.EventType));
                await eventPublisher.PublishAsync(evtDeserialized);
            }, userCredentials: new UserCredentials("admin", "changeit")).GetAwaiter().GetResult();
        }

        public async Task<IEnumerable<Event>> GetEventsAsync(string eventStreamId)
        {
            Trace.Assert(!string.IsNullOrEmpty(eventStreamId));
            var events = await conn.ReadStreamEventsForwardAsync(eventStreamId, 0, 4096, true);

            return events.Events.Select(e => e.Event.Data.DeserializeEvent(Type.GetType(e.Event.EventType)));
        }

        public async Task SaveEventsAsync(string eventStreamId, IEnumerable<Event> events, long expectedVersion)
        {
            Trace.Assert(!string.IsNullOrEmpty(eventStreamId));
            Trace.Assert(events != null);
            var serializedEvents = events.Select(e => new EventData(Guid.NewGuid(), e.GetType().FullName, true, e.SerializeEvent(), null)).ToArray();
            await conn.AppendToStreamAsync(eventStreamId, expectedVersion - 1, serializedEvents);
        }
    }

    public static class EventStoreSerializationEx
    {
        public static Event DeserializeEvent(this byte[] data, Type eventType) => (Event)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), eventType);

        public static byte[] SerializeEvent(this Event evt) => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(evt));
    }

    public static class ESEvenStoreConfigEx
    {
        public static IServiceCollection AddESEventStore(this IServiceCollection services)
        {
            services.AddSingleton<IEventStore, ESEventStore>();

            var node = EmbeddedVNodeBuilder
                .AsSingleNode()
                .OnDefaultEndpoints()
                .RunInMemory()
                .Build();

            node.Start();

            services.AddSingleton<IEventStoreConnection>(sp =>
            {
                //var settings = ConnectionSettings
                //    .Create()
                //    .UseConsoleLogger()
                //    .EnableVerboseLogging()
                //    //.FailOnNoServerResponse()
                //    .DisableServerCertificateValidation()
                //    .LimitRetriesForOperationTo(1)
                //    .Build();
                //var conn = EventStoreConnection.Create(settings, new Uri("tcp://admin:changeit@localhost:1113"));
                var conn = EmbeddedEventStoreConnection.Create(node);
                conn.ConnectAsync().GetAwaiter().GetResult();

                return conn;
            });

            return services;
        }
    }
}
