using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Invoria.BuildingBlocks.Core.Modularity
{
    public interface IServiceInstaller
    {
        void Install(IServiceCollection services, IConfiguration configuration);
    }
}
