using System.Diagnostics;
using datotekica;
using datotekica.Auth;
using datotekica.Entities;
using datotekica.Extensions;
using datotekica.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Company.WebApplication1;

public class Program
{
    public static async Task Main(string[] args)
    {
        InitializeDirectories();
        await InitializeDb();

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.All;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        builder.Services.AddDbContextFactory<AppDbContext>(builder =>
        {
            builder.UseSqlite(C.Paths.AppDbConnectionString);
            if (Debugger.IsAttached)
            {
                builder.EnableSensitiveDataLogging();
                builder.LogTo(message => Debug.WriteLine(message), new[] { RelationalEventId.CommandExecuted });
            }
        });
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();
        builder.Services.AddDataProtection().PersistKeysToDbContext<AppDbContext>();
        builder.Services.AddSingleton<CacheService>();
        builder.Services.AddSingleton<MailService>();
        builder.Services.AddScoped<ToastService>();
        builder.Services.AddAuthentication(HeaderAuthenticationOptions.DEFAULT_SCHEME).AddHeader();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
        app.UseForwardedHeaders();
        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapDownload();
        app.MapBlazorHub().RequireAuthorization();
        app.MapFallbackToPage("/_Host");

        app.Run();
    }
    static void InitializeDirectories()
    {
        var config = new DirectoryInfo(C.Paths.Config);
        config.Create();
        var data = new DirectoryInfo(C.Paths.Data);
        data.Create();
    }
    static async Task InitializeDb()
    {
        using var db = GetDb();
        if (db.Database.GetMigrations().Any())
            await db.Database.MigrateAsync();
        else
            await db.Database.EnsureCreatedAsync();

        // Seed
        // if (!db.Users.Any())
        //     await db.InitializeDefaults();
    }
    static AppDbContext GetDb()
    {
        var opt = new DbContextOptionsBuilder<AppDbContext>();
        opt.UseSqlite(C.Paths.AppDbConnectionString);

        return new AppDbContext(opt.Options);
    }
}