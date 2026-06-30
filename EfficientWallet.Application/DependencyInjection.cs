using EfficientWallet.Application.Common.Interfaces;
using EfficientWallet.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EfficientWallet.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        => services.AddApplicationServices();

        private static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IBalanceConverter, BalanceConverter>();
            return services;
        }
    }
}
