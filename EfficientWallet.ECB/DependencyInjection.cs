using EfficientWallet.ECB.Http;
using EfficientWallet.ECB.Http.Settings;
using EfficientWallet.ECB.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;

namespace EfficientWallet.ECB
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddECB(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddServices()
            .AddHttpClients(configuration);

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IEcbService, EcbService>();
            return services;
        }

        private static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ECBClientSettings>(configuration.GetSection(ECBClientSettings.Section));
            services.AddRefitClient<IEcbClient>(new RefitSettings
            {
                ContentSerializer = new XmlContentSerializer() //Ecb client returns an xml response
            })
            .ConfigureHttpClient((sp, client) =>
            {
                ECBClientSettings settings = sp.GetRequiredService<IOptions<ECBClientSettings>>().Value;

                client.BaseAddress = new Uri(settings.BaseAddress);
                client.Timeout = TimeSpan.FromSeconds(settings.TimeoutInSeconds);
            });
            return services;
        }

    }
}
