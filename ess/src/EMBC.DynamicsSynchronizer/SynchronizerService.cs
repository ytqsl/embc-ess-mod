using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventStore.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EMBC.DynamicsSynchronizer
{
    public class SynchronizerService : IHostedService, IDisposable
    {
        private bool disposedValue;
        private StreamSubscription subscription;
        private readonly EventStoreClient conn;
        private readonly ILogger logger;

        public SynchronizerService(EventStoreClient conn, ILogger<SynchronizerService> logger)
        {
            this.conn = conn;
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            subscription = await conn.SubscribeToAllAsync(Position.Start,
                 async (sub, evt, _) =>
                {
                    await Task.CompletedTask;
                    logger.LogDebug("Sending {0} to Dynamics: {1}", evt.Event.EventType, Encoding.UTF8.GetString(evt.Event.Data.Span.ToArray()));
                }, true,
                (sub, reason, ex) =>
                {
                    logger.LogError(ex, "subscription dropped: {0}", reason);
                },
                new SubscriptionFilterOptions(EventTypeFilter.ExcludeSystemEvents()));
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            subscription.Dispose();
            subscription = null;
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
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
