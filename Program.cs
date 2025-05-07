using Alhadis.Models;
using Microsoft.EntityFrameworkCore;

namespace Alhadis
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
            builder.WebHost.UseUrls($"http://0.0.0.0:{port}");


            // Add services to the container.

            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<HadithDbContext>(options =>
         options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
