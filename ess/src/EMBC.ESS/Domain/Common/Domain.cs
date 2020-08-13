using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace EMBC.ESS.Domain.Common
{
    #region Messaging

    public interface IMessage { }

    public interface ICommand : IMessage { }

    public interface ICommand<out TResponse> : ICommand { }

    public abstract class Event : IMessage
    {
        public ulong Version;
    }

    public interface ICommandSender
    {
        Task SendAsync(ICommand command);

        Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command);
    }

    public interface IEventPublisher
    {
        Task PublishAsync<T>(T evt) where T : Event;
    }

    #endregion Messaging

    #region Event sourcing

    public interface IEventStore
    {
        Task SaveEventsAsync(string stream, IEnumerable<Event> events, ulong expectedVersion);

        IAsyncEnumerable<Event> GetEventsAsync(string stream);
    }

#pragma warning disable CA1032, S3925

    public class StreamNotFoundException : Exception
    {
        public StreamNotFoundException(string streamId)
            : base($"StreamId {streamId}")
        {
        }
    }

    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(string streamId, long expectedVersion, long actualVersion)
            : base($"Stream {streamId} expected to be in version {expectedVersion} but was version {actualVersion}")
        {
        }
    }

#pragma warning restore CA1032, S3925

    #endregion Event sourcing

    #region Domain

    public interface IRepository<TItem> where TItem : AggregateRoot
    {
        Task SaveAsync(TItem aggregate, ulong expectedVersion);

        Task SaveAsync(TItem aggregate);

        Task<TItem> GetByIdAsync(string id);
    }

    //public interface IReadModelRepository<TItem> where TItem : AggregateRoot
    //{
    //    IAsyncEnumerable<TItem> GetAsync(Func<TItem, bool> filter = null);

    //    Task<TItem> GetByIdAsync(string id);
    //}
    public interface IReadModelRepository<TReadModel>
    {
        Task SetAsync(string key, TReadModel item);

        IAsyncEnumerable<TReadModel> GetAsync(Func<TReadModel, bool> filter = null);

        Task<TReadModel> GetByKeyAsync(string key);
    }

    public class InMemoryReadModelRepository<TReadModel> : IReadModelRepository<TReadModel>
    {
        private ConcurrentDictionary<string, TReadModel> data = new ConcurrentDictionary<string, TReadModel>();

        public IAsyncEnumerable<TReadModel> GetAsync(Func<TReadModel, bool> filter = null)
        {
            return data.Values.Where(filter).ToAsyncEnumerable();
        }

        public async Task<TReadModel> GetByKeyAsync(string key)
        {
            await Task.CompletedTask;
            return data.GetValueOrDefault(key);
        }

        public async Task SetAsync(string key, TReadModel item)
        {
            await Task.CompletedTask;
            data.AddOrUpdate(key, item, (k, v) => item);
        }
    }

    public abstract class AggregateRoot
    {
        private readonly List<Event> changes = new List<Event>();
        public string Id { get; protected set; }
        public ulong Version { get; private set; } = ulong.MaxValue;

        internal IEnumerable<Event> GetUncommittedChanges()
        {
            if (Id.Equals(default))
            {
                throw new InvalidOperationException($"Id was not set for aggregate, cannot persist it");
            }
            return changes;
        }

        internal void MarkChangesAsCommitted()
        {
            changes.Clear();
        }

        internal async Task LoadsFromHistory(IAsyncEnumerable<Event> history)
        {
            await foreach (var e in history)
            {
                ApplyChange(e, false);
                Version = e.Version;
            }
        }

        protected void ApplyChange(Event @event)
        {
            ApplyChange(@event, true);
        }

        private void ApplyChange(Event @event, bool isNew)
        {
            //Call internal Apply(event) methods in the aggregate
            this.GetType().InvokeMember("Apply", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic, null, this, new object[] { @event });
            if (isNew)
            {
                changes.Add(@event);
            }
        }
    }

    public class Repository<TItem> : IRepository<TItem> where TItem : AggregateRoot
    {
        private readonly IEventStore _storage;
        private readonly Func<Task<TItem>> factory;

        public Repository(IEventStore storage) : this(storage, () => Activator.CreateInstance<TItem>())
        {
        }

        public Repository(IEventStore storage, Func<TItem> factory) : this(storage, () => Task.FromResult(factory()))
        {
        }

        public Repository(IEventStore storage, Func<Task<TItem>> factory)
        {
            _storage = storage;
            this.factory = factory;
        }

        public async Task SaveAsync(TItem aggregate, ulong expectedVersion)
        {
            if (aggregate is null) { throw new ArgumentNullException(nameof(aggregate)); }

            await _storage.SaveEventsAsync(GetStreamName(aggregate.Id), aggregate.GetUncommittedChanges(), expectedVersion);
            aggregate.MarkChangesAsCommitted();
        }

        public async Task SaveAsync(TItem aggregate)
        {
            if (aggregate is null) { throw new ArgumentNullException(nameof(aggregate)); }
            await SaveAsync(aggregate, aggregate.Version);
        }

        public async Task<TItem> GetByIdAsync(string id)
        {
            var aggregate = await factory();
            var events = _storage.GetEventsAsync(GetStreamName(id));
            await aggregate.LoadsFromHistory(events);
            return aggregate;
        }

        private static string GetStreamName(string id) => $"{typeof(TItem).FullName}-{id}";
    }

    public abstract class DomainException : ApplicationException
    {
        protected DomainException()
        {
        }

        protected DomainException(string message) : base(message)
        {
        }

        protected DomainException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DomainException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public interface IProvideSequenceNumbers
    {
        Task<ulong> NextAsync<TItem>() where TItem : AggregateRoot;
    }

    #endregion Domain
}
