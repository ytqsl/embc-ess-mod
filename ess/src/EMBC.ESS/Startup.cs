using EMBC.ESS.Domain.Common;
using EMBC.ESS.Domain.Common.EventStore;
using EMBC.ESS.Domain.ReadModels;
using EMBC.ESS.Domain.Registrants;
using EMBC.ESS.Domain.Supports;
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
            services.AddESEventStore();
            services.AddInMemoryMediator();
            services.AddTransient<IRepository<Registration>, Repository<Registration>>();
            services.AddTransient<IRepository<SupportsFile>, Repository<SupportsFile>>();
            services.AddTransient<IRepository<SupportsRequest>, Repository<SupportsRequest>>();
            services.AddTransient<ISupportsFileFactory, SupportFileFactory>();
            services.AddSingleton<IReadModelRepository<RegistrantProfileView>, InMemoryReadModelRepository<RegistrantProfileView>>();
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
            app.UseDefaultFiles();
            app.UseStatusCodePages();

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
