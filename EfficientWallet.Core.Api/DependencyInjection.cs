using OpenMediator;
using System.Reflection;

namespace EfficientWallet.Core.Api
{
    internal static class DependencyInjection
    {
        internal static IServiceCollection AddPresentation(this IServiceCollection services)
        {
            services.AddSwaggerGen();
            services.AddEndpointsApiExplorer();
            services.AddControllers();

            services.AddOpenMediator(config =>
            config.RegisterCommandsFromAssemblies(
                Assembly.GetExecutingAssembly(),
                typeof(Application.DependencyInjection).Assembly
            ));
            return services;
        }
    }
}
