using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SchoolApp.Data;
using SchoolApp.Repositories;
using SchoolApp.Services;
using Serilog;

namespace SchoolApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        var connString = builder.Configuration.GetConnectionString("DefaultConnection");
        connString = connString!.Replace("{DB_PASS}", Environment.GetEnvironmentVariable("DB_PASS") ?? "");
        
        builder.Services.AddDbContext<SchoolAppDbContext>(options => options.UseSqlServer(connString));
        builder.Services.AddRepositories();
        builder.Services.AddScoped<IApplicationService, ApplicationService>();

        builder.Services.AddAutoMapper(cfg => cfg.AddProfile<Configuration.MapperConfig>());
        builder.Host.UseSerilog((context, config) =>
        {
            config.ReadFrom.Configuration(context.Configuration);
        });
        
        // Add services to the container.
        builder.Services.AddControllersWithViews();   // αν θελουμε DI όλα τα services τα βαζουμε εδω (μέσα στον builder)
        
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/User/Login";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;   // reset timeout, 30 min of idle
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
        app.UseStaticFiles();

        app.UseRouting();
        
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}"); // εδω βάζουμε το landing page

        app.Run();
    }
}