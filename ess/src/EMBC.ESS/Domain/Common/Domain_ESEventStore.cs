using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace EMBC.ESS.Domain.Common
{
    public class ESEventStore : IEventStore
    {
        private readonly IEventStoreConnection conn;
        private readonly IEventPublisher eventPublisher;

        public ESEventStore(IEventStoreConnection conn, IEventPublisher eventPublisher)
        {
            this.conn = conn;
            this.eventPublisher = eventPublisher;
        }

        public async Task<IEnumerable<Event>> GetEventsAsync(string eventStreamId)
        {
            var streamId = eventStreamId.ToString();
            var events = await conn.ReadStreamEventsForwardAsync(streamId, 0, 4096, true);

            return events.Events.Select(e => e.Event.Data.DeserializeEvent(Type.GetType(e.Event.EventType)));
        }

        public async Task SaveEventsAsync(string eventStreamId, IEnumerable<Event> events, long expectedVersion)
        {
            var serializedEvents = events.Select(e => new EventData(Guid.NewGuid(), e.GetType().FullName, true, e.SerializeEvent(), null)).ToArray();
            await conn.AppendToStreamAsync(eventStreamId.ToString(), expectedVersion - 1, serializedEvents);
            foreach (var evt in events)
            {
                await eventPublisher.PublishAsync(evt);
            }
        }
    }

    public static class EventStoreSerializationEx
    {
        public static Event DeserializeEvent(this byte[] data, Type eventType) => (Event)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), eventType);

        public static byte[] SerializeEvent(this Event evt) => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(evt));
    }
}
