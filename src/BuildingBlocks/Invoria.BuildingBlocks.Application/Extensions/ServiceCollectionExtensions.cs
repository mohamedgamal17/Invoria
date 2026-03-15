using Invoria.BuildingBlocks.Application.Factories;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
namespace Invoria.BuildingBlocks.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterFactoriesFromAssembly(this IServiceCollection services, Assembly assembly)
        {
            var types = assembly.GetTypes()
            .Where(x => x.IsClass)
            .Where(x => x.GetInterfaces().Any(c => c.IsGenericType && c.GetGenericTypeDefinition() == typeof(IResponseFactory<,>)))
            .ToList();

            foreach (var type in types)
            {
                services.AddTransient(type.GetInterfaces().Where(x => !x.IsGenericType).First(), type);
            }

            return services;
        }
    }
}
