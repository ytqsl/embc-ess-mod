using System;
using System.Threading.Tasks;
using Jasper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Debugging;

namespace EMBC.ESS
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            SelfLog.Enable(Console.Error);
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseJasper<ApiJasperOptions>()
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }

    public class ApiJasperOptions : JasperOptions
    {
        public ApiJasperOptions()
        {
            Endpoints.DefaultLocalQueue.NotDurable();
        }
    }
}
