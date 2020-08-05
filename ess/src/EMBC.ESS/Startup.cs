using EMBC.ESS.Domain.Common;
using EMBC.ESS.Domain.Registrants;
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
            //services.AddEmbeddedESEventStore();
            services.AddESEventStore();
            services.AddJasperMessageBus();
            services.AddTransient<IRepository<Registration>, Repository<Registration>>();
            services.AddTransient<IReadModelRepository<Registration>, ESReadModelRepository<Registration>>();
            //services.AddTransient<IRegistrantProfileReadModelRepository, RegistrantProfileReadModelRepository>();
            //services.AddTransient<RegistrantProfileReadModelBuilder>();
            services.AddTransient<IRegistrantProfileReadModelRepository, ESRegistrationProfileReadModel>();
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
            app.InitializeESEventStore(
                //handlersToReplay: new[] { typeof(ReadModelEventHandler) },
                //readModelBuilders: new[] { typeof(RegistrantProfileReadModelBuilder) }
                );
        }
    }
}
