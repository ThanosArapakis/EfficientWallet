using EfficientWallet.Application.Common.Interfaces;
using EfficientWallet.ECB;
using EfficientWallet.Infrastructure.Persistence;
using EfficientWallet.Infrastructure.Persistence.Repository;
using EfficientWallet.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EfficientWallet.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        => services.AddServices()
                   .AddDatabase(configuration)
                   .AddRepositories()
                   .AddECB(configuration);


        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton<IRateCache, RatesMemoryCache>();
            return services;
        }

        private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sql => sql.EnableRetryOnFailure());
            });
            return services;
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
            services.AddScoped<IWalletRepository, WalletRepository>();
            return services;
        }

    }
}
