using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Invoria.BuildingBlocks.Core.Modularity
{
    public interface IModuleInstaller
    {
        void Install(IServiceCollection services, IConfiguration configuration);

    }
}
