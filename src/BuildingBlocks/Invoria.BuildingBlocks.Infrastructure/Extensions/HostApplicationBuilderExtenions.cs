using Invoria.BuildingBlocks.Core.Modularity;
using Microsoft.Extensions.Hosting;
namespace Invoria.BuildingBlocks.Infrastructure.Extensions
{
    public static class HostApplicationBuilderExtenions
    {
        public static IHostApplicationBuilder InstallModule<T>(this IHostApplicationBuilder host) where
            T : class, IModuleInstaller
        {
            var moduleType = typeof(T);

            var module = ((IModuleInstaller)Activator.CreateInstance(moduleType)!)!;

            module.Install(host.Services, host.Configuration);

            return host;
        }
    }
}
