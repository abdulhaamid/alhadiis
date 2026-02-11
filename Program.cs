using Alhadis;
using Alhadis.Middleware;
using Alhadis.Models;
using Alhadis.Services;
using Microsoft.EntityFrameworkCore;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllersWithViews();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ITenantContext, TenantContext>();
        builder.Services.AddScoped<IStockService, StockService>();

        var sqlServerConnection = builder.Configuration.GetConnectionString("SqlServerConnection");
        var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");

        builder.Services.AddDbContext<HadithDbContext>(options =>
        {
            if (!string.IsNullOrWhiteSpace(sqlServerConnection))
            {
                options.UseSqlServer(sqlServerConnection, sql => sql.EnableRetryOnFailure());
            }
            else if (!string.IsNullOrWhiteSpace(defaultConnection))
            {
                options.UseNpgsql(defaultConnection);
            }
            else
            {
                options.UseSqlite("Data Source=alhadis.db");
            }
        });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        });

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HadithDbContext>();
            dbContext.Database.Migrate();
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseTenantResolution();

        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.Run();
    }
}
