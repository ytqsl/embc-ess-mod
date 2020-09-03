using System;
using System.Net.Http;
using System.Threading.Tasks;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Debugging;

namespace EMBC.DynamicsSynchronizer
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            SelfLog.Enable(Console.Error);
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
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
                    services.AddLogging();
                    services.AddHostedService<SynchronizerService>();
                })
                .UseSerilog();

            await builder.RunConsoleAsync();
        }
    }
}
