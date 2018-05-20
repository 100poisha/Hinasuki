using Hinasuki.Hubs;
using Hinasuki.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hinasuki
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            Settings.ConnectionString = Configuration.GetConnectionString("DefaultConnection");

            services.AddMemoryCache();
            services.AddMvc();
            services.AddSignalR()
                .AddAzureSignalR();

            var connectionString = Configuration.GetConnectionString("DefaultConnection");

            services.AddSingleton<HinasukiRepository>(_ => new HinasukiRepository(connectionString));
            services.AddSingleton<HinasukiHubValue>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "default", template: "{controller=Home}/{action=Index}/{id?}");
            });
            app.UseAzureSignalR(routes =>
            {
                routes.MapHub<HinasukiHub>("/Hinasuki");
            });
        }
    }
}
