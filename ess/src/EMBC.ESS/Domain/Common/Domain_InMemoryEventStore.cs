using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EMBC.ESS.Domain.Common
{
    public class InMemoryEventStore : IEventStore
    {
        private readonly IEventPublisher publisher;

        public InMemoryEventStore(IEventPublisher publisher)
        {
            this.publisher = publisher;
        }

        private readonly ConcurrentDictionary<string, InMemoryEventStream> current = new ConcurrentDictionary<string, InMemoryEventStream>();

        private struct InMemoryEventStream
        {
            public List<Event> Events { get; set; }
        }

        public async Task SaveEventsAsync(string eventStreamId, IEnumerable<Event> events, long expectedVersion)
        {
            // try to get event descriptors list for given stream
            if (!current.TryGetValue(eventStreamId, out var stream))
            {
                // otherwise -> create empty stream
                stream = new InMemoryEventStream { Events = new List<Event>() };
                if (!current.TryAdd(eventStreamId, stream))
                {
                    throw new InvalidOperationException($"Failed to add event stream {eventStreamId} to in memory dictionary");
                }
            }
            else
            {
                // check whether latest event version matches current stream version
                // otherwise -> throw exception
                var actualVersion = stream.Events.Last().Version;
                if (actualVersion != expectedVersion && expectedVersion != -1)
                {
                    throw new ConcurrencyException(eventStreamId, expectedVersion, actualVersion);
                }
            }
            var i = expectedVersion; var publishQueue = new List<Event>();
            // iterate through current stream events increasing version with each processed event
            foreach (var evt in events)
            {
                i++; evt.Version = i;
                // push event to the event descriptors list for current stream
                stream.Events.Add(evt);
                // publish current event to the bus for further processing by subscribers
                publishQueue.Add(evt);
            }
            Task.WaitAll(publishQueue.Select(e => publisher.PublishAsync(e)).ToArray());
            await Task.CompletedTask;
        }

        // collect all processed events for given stream and return them as a list
        // used to build up an aggregate from its history (Domain.LoadsFromHistory)
        public async Task<IEnumerable<Event>> GetEventsAsync(string eventStreamId)
        {
            await Task.CompletedTask;
            if (!current.TryGetValue(eventStreamId, out var stream))
            {
                throw new StreamNotFoundException(eventStreamId);
            }
            return stream.Events.ToArray();
        }
    }
}
