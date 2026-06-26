using Microsoft.Extensions.DependencyInjection;

namespace EfficientWallet.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        => services.AddApplicationServices();

        private static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            return services;
        }
    }
}
