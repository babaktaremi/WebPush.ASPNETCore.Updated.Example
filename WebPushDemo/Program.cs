using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebPushDemo.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<WebPushDemoContext>(options =>
    options.UseSqlite("Data Source=Data/WebPushDb.db"));

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    //app.UseBrowserLink();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

await EnsureDatabaseCreation(app);

app.UseStaticFiles();

if (app.Configuration.GetSection("VapidKeys")["PublicKey"]?.Length == 0 || app.Configuration.GetSection("VapidKeys")["PrivateKey"]?.Length == 0)
{
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=WebPush}/{action=GenerateKeys}/{id?}");

}
else
{
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Devices}/{action=Index}/{id?}");
}
await app.RunAsync();

async Task EnsureDatabaseCreation(IApplicationBuilder app)
{
    using var serviceScope = app.ApplicationServices.CreateScope();
    var context = serviceScope.ServiceProvider.GetRequiredService<WebPushDemoContext>();
    await context.Database.MigrateAsync();
}
