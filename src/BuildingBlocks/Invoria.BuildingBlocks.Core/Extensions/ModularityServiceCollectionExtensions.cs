using Invoria.BuildingBlocks.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
namespace Invoria.BuildingBlocks.Core.Extensions
{
    public static class ModularityServiceCollectionExtensions
    {
        public static IServiceCollection InstallServiceFromAssembly(this IServiceCollection services, IConfiguration configuration, params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Count() < 1)
            {
                return services;
            }

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes()
                     .Where(x => x.IsClass && x.IsAssignableTo(typeof(IServiceInstaller)))
                     .ToList();

                types.ForEach((t) => services.InstallService(t, configuration));
            }

            return services;
        }

        public static IServiceCollection InstallService<TService>(this IServiceCollection services, IConfiguration configuration)
        {
            return services.InstallService(typeof(TService), configuration);
        }

        public static IServiceCollection InstallService(this IServiceCollection services, Type serviceType, IConfiguration configuration)
        {
            if (serviceType.IsAssignableFrom(typeof(IServiceInstaller)))
            {
                throw new InvalidOperationException($"[{serviceType.AssemblyQualifiedName}] must implement type of [{typeof(IServiceInstaller).AssemblyQualifiedName}]");
            }


            var hasEmptyConstructor = serviceType.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                 .Where(x => x.GetParameters().Length == 0)
                 .Any();

            if (!hasEmptyConstructor)
            {
                throw new InvalidOperationException($"[{serviceType.AssemblyQualifiedName}] must have parameterless constructor to be able to install module");
            }

            var obj = (IServiceInstaller)Activator.CreateInstance(serviceType, new object[] { })!;

            obj.Install(services, configuration);

            return services;
        }

        public static IServiceCollection InstallModulesFromAssembly(this IServiceCollection services, IConfiguration configuration, params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Count() < 1)
            {
                return services;
            }

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes()
                     .Where(x => x.IsClass && x.IsAssignableTo(typeof(IModuleInstaller)))
                     .ToList();

                types.ForEach((t) => services.InstallModule(t, configuration));
            }

            return services;
        }

        public static IServiceCollection InstallModule<TModule>(this IServiceCollection services, IConfiguration configuration)
            where TModule : IModuleInstaller
        {
            return services.InstallModule(typeof(TModule), configuration);
        }

        public static IServiceCollection InstallModule(this IServiceCollection services, Type moduleType, IConfiguration configuration)
        {
            if (moduleType.IsAssignableFrom(typeof(IModuleInstaller)))
            {
                throw new InvalidOperationException($"[{moduleType.AssemblyQualifiedName}] must implement type of [{typeof(IModuleInstaller).AssemblyQualifiedName}]");
            }

            var hasEmptyConstructor = moduleType.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                 .Where(x => x.GetParameters().Length == 0)
                 .Any();

            if (!hasEmptyConstructor)
            {
                throw new InvalidOperationException($"[{moduleType.AssemblyQualifiedName}] must have parameterless constructor to be able to install module");
            }

            var obj = (IModuleInstaller)Activator.CreateInstance(moduleType, new object[] { })!;

            obj.Install(services, configuration);

            return services;
        }
    }
}
