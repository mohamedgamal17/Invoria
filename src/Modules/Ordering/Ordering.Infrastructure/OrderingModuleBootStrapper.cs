using Invoria.BuildingBlocks.Core.Modularity;

namespace Invoria.Ordering.Infrastructure
{
    public class OrderingModuleBootStrapper : IModuleBootstrapper
    {
        public Task Bootstrap(IServiceProvider serviceProvider)
        {
            return Task.CompletedTask;
        }
    }
}
