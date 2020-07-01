using System;
using EMBC.ESS.Domain.Common;
using EMBC.ESS.Domain.Profiles;
using EventStore.ClientAPI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EMBC.ESS
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddDataProtection().UseEphemeralDataProtectionProvider();
            //services.AddSingleton<IEventStore, InMemoryEventStore>();
            services.AddTransient<IEventStore, ESEventStore>();
            services.AddSingleton<IEventStoreConnection>(sp =>
            {
                var settings = ConnectionSettings
                    .Create()
                    .UseConsoleLogger()
                    //.EnableVerboseLogging()
                    //.FailOnNoServerResponse()
                    .DisableServerCertificateValidation()
                    .LimitRetriesForOperationTo(2)
                    .Build();
                var conn = EventStoreConnection.Create(settings, new Uri("tcp://admin:changeit@localhost:1113"));
                conn.ConnectAsync().GetAwaiter().GetResult();

                return conn;
            });
            services.AddTransient<IRepository<Profile>, Repository<Profile>>();
            services.AddTransient<IBus, JasperServiceBus>();
            services.AddTransient<IEventPublisher, JasperServiceBus>();
            services.AddTransient<ICommandSender, JasperServiceBus>();
            services.AddTransient<IProfileReadModelRepository, ProfileReadModelRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute(name: "areas", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
