using Microsoft.Extensions.DependencyInjection;
namespace Invoria.BuildingBlocks.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Replace<TService, TImplementaion>(this IServiceCollection services)
        {
            return services.Replace(typeof(TService), typeof(TImplementaion));
        }

        public static IServiceCollection Replace(this IServiceCollection services, Type service, Type implementaion)
        {
            var oldService = services.SingleOrDefault(x => x.ServiceType == service);

            if (oldService != null)
            {
                services.Remove(oldService);

            }

            var serviceDescriptor = new ServiceDescriptor(service, implementaion, oldService?.Lifetime ?? ServiceLifetime.Transient);

            services.Add(serviceDescriptor);

            return services;
        }

        public static T? GetSinglatonOrNull<T>(this IServiceCollection services)
        {
            return (T)services.GetSinglatonOrNull(typeof(T));
        }
        public static object? GetSinglatonOrNull(this IServiceCollection services, Type targetType)
        {
            var descriptor = services.FirstOrDefault(
                d => d.ServiceType == targetType && d.Lifetime == ServiceLifetime.Singleton);

            return descriptor?.ImplementationInstance;
        }
    }
}
