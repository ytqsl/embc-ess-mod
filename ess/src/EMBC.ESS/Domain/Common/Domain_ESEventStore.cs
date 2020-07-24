using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Embedded;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace EMBC.ESS.Domain.Common
{
    public class ESEventStore : IEventStore
    {
        private readonly IEventStoreConnection conn;

        public ESEventStore(IEventStoreConnection conn)
        {
            this.conn = conn ?? throw new ArgumentNullException(nameof(conn));
        }

        public async IAsyncEnumerable<Event> GetEventsAsync(string eventStreamId)
        {
            Trace.Assert(!string.IsNullOrEmpty(eventStreamId));
            long lastEventNumber = 0;
            int chunkSize = 4096;
            StreamEventsSlice eventsSlice;
            do
            {
                eventsSlice = await conn.ReadStreamEventsForwardAsync(eventStreamId, lastEventNumber, chunkSize, true);
                lastEventNumber = eventsSlice.NextEventNumber;
                foreach (var e in eventsSlice.Events)
                {
                    yield return e.ToDomainEvent();
                }
            } while (!eventsSlice.IsEndOfStream);
        }

        public async Task SaveEventsAsync(string eventStreamId, IEnumerable<Event> events, long expectedVersion)
        {
            Trace.Assert(!string.IsNullOrEmpty(eventStreamId));
            Trace.Assert(events != null);
            var serializedEvents = events.Select(e => new EventData(Guid.NewGuid(), e.GetType().FullName, true, e.SerializeEvent(), null)).ToArray();
            if (expectedVersion == 0) expectedVersion--;
            await conn.AppendToStreamAsync(eventStreamId, expectedVersion, serializedEvents);
        }
    }

    public static class EventStoreEx
    {
        public static Event DeserializeEvent(this byte[] data, Type eventType) => (Event)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), eventType);

        public static byte[] SerializeEvent(this Event evt) => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(evt));

        public static Event ToDomainEvent(this ResolvedEvent e)
        {
            var evt = e.Event.Data.DeserializeEvent(Type.GetType(e.Event.EventType));
            evt.Version = e.Event.EventNumber;
            return evt;
        }
    }

    public static class ESEvenStoreConfigEx
    {
        public static IServiceCollection AddEmbeddedESEventStore(this IServiceCollection services)
        {
            var node = EmbeddedVNodeBuilder
                .AsSingleNode()
                .OnDefaultEndpoints()
                //.RunInMemory()
                .RunOnDisk("./EventStore")
                .Build();

            node.Start();

            var conn = EmbeddedEventStoreConnection.Create(node);
            conn.ConnectAsync().GetAwaiter().GetResult();

            services.AddSingleton(conn);
            services.AddSingleton<IEventStore, ESEventStore>();

            return services;
        }

        public static IServiceCollection AddESEventStore(this IServiceCollection services)
        {
            var settings = ConnectionSettings
            .Create()
            .UseConsoleLogger()
            //.EnableVerboseLogging()
            .FailOnNoServerResponse()
            .DisableServerCertificateValidation()
            .LimitRetriesForOperationTo(1)
            .DisableTls()
            .Build();
            var conn = EventStoreConnection.Create(settings, new Uri("tcp://admin:changeit@localhost:1113"));
            conn.ConnectAsync().GetAwaiter().GetResult();

            services.AddSingleton(conn);
            services.AddSingleton<IEventStore, ESEventStore>();
            return services;
        }

        public static IApplicationBuilder InitializeESEventStore(this IApplicationBuilder builder)
        {
            var eventPublisher = builder.ApplicationServices.GetRequiredService<IEventPublisher>();
            var conn = builder.ApplicationServices.GetRequiredService<IEventStoreConnection>();

            conn.FilteredSubscribeToAllFrom(null, Filter.ExcludeSystemEvents, CatchUpSubscriptionFilteredSettings.Default,
               async (sub, evt) =>
               {
                   await eventPublisher.PublishAsync(evt.ToDomainEvent());
               });
            return builder;
        }
    }
}
