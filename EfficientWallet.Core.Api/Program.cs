using EfficientWallet.Application;
using EfficientWallet.Application.Common.Interfaces;
using EfficientWallet.Application.Services;
using EfficientWallet.Core.Api.BackgroundServices;
using EfficientWallet.Infrastructure;
using EfficientWallet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EfficientWallet.Core.Api;

public static class Program
{
    public static void Main(string[] args)
    {
        try
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services
            .AddPresentation()
            .AddApplication()
            .AddInfrastructure(builder.Configuration);

            //background service for fetching the exchange rates once in a minute
            builder.Services.AddHostedService<ExchangeRateSyncWorker>();
            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton<IRateCache, RatesMemoryCache>();

            var app = builder.Build();

            // Ensure the database exists and is on the latest schema before serving traffic.
            // Creates EfficientWalletDB on a fresh SQL Server container and applies migrations.
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.Migrate();
            }

            app.Configure(app.Environment);

            app.Run();
        }
        catch (Exception ex)
        {
            //Log the exception details to the console or a log file for debugging purposes
            Console.WriteLine($"An error occurred while starting the application: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            throw;
        }
        finally
        {
            Console.WriteLine("Application has stopped.");
        }
    }

    public static void Configure(this WebApplication app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment() || env.IsEnvironment("Debug"))
        {
            app.UseDeveloperExceptionPage();

            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseHttpsRedirection();

        app.UseRouting();
        app.UseAuthorization();

        if (env.IsDevelopment() || env.IsEnvironment("Debug"))
        {
            app.UseCors(x => x.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost").AllowAnyMethod().AllowAnyHeader());
        }

        app.MapControllers();
    }

}