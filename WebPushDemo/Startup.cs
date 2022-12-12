using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using WebPushDemo.Models;

namespace WebPushDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            
            services.AddDbContext<WebPushDemoContext>(options =>
                options.UseSqlite("Data Source=Data/WebPushDb.db"));

            services.AddSingleton<IConfiguration>(Configuration);
            services.Configure<MvcOptions>(c =>
            {
                c.EnableEndpointRouting = false;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                //app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            EnsureDatabaseCreation(app);

            app.UseStaticFiles();

            if (Configuration.GetSection("VapidKeys")["PublicKey"].Length == 0 || Configuration.GetSection("VapidKeys")["PrivateKey"].Length == 0)
            {
                app.UseMvc(routes =>
                {
                    routes.MapRoute(
                        name: "default",
                        template: "{controller=WebPush}/{action=GenerateKeys}/{id?}");
                });

                return;
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Devices}/{action=Index}/{id?}");
            });
        }

        private void EnsureDatabaseCreation(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            var context = serviceScope.ServiceProvider.GetRequiredService<WebPushDemoContext>();
            context.Database.Migrate();
        }
    }
}
