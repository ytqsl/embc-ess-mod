using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace EMBC.ESS.Domain.Common
{
    #region Messaging

    public interface IMessage { }

    public interface ICommand : IMessage { }

    public interface ICommand<out TResponse> : ICommand { }

    public interface IQuery<out TResponse> : IMessage { }

    public abstract class Event : IMessage
    {
        public long Version;
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

    public interface IQuerySender
    {
        Task<TResponse> QueryAsync<TResponse>(IQuery<TResponse> command);
    }

    public interface IBus : ICommandSender, IEventPublisher, IQuerySender { }

    #endregion Messaging

    #region Event sourcing

    public interface IEventStore
    {
        Task SaveEventsAsync(string eventStreamId, IEnumerable<Event> events, long expectedVersion);

        IAsyncEnumerable<Event> GetEventsAsync(string eventStreamId);
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
        Task SaveAsync(TItem aggregate, long expectedVersion);

        Task SaveAsync(TItem aggregate);

        Task<TItem> GetByIdAsync(Guid id);
    }

    public abstract class AggregateRoot
    {
        private readonly List<Event> changes = new List<Event>();
        public Guid Id { get; protected set; }
        public long Version { get; private set; }

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

        public async Task SaveAsync(TItem aggregate, long expectedVersion)
        {
            if (aggregate is null) { throw new ArgumentNullException(nameof(aggregate)); }

            await _storage.SaveEventsAsync(GetStreamName(aggregate), aggregate.GetUncommittedChanges(), expectedVersion);
            aggregate.MarkChangesAsCommitted();
        }

        public async Task SaveAsync(TItem aggregate)
        {
            if (aggregate is null) { throw new ArgumentNullException(nameof(aggregate)); }
            await SaveAsync(aggregate, aggregate.Version);
        }

        public async Task<TItem> GetByIdAsync(Guid id)
        {
            var aggregate = await factory();
            var events = _storage.GetEventsAsync(GetStreamName(aggregate, id));
            await aggregate.LoadsFromHistory(events);
            return aggregate;
        }

        private static string GetStreamName(TItem aggregate)
        {
            return GetStreamName(aggregate, aggregate.Id);
        }

        private static string GetStreamName(TItem aggregate, Guid id)
        {
            return $"{aggregate.GetType().FullName}_{id}";
        }
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

    #endregion Domain
}
