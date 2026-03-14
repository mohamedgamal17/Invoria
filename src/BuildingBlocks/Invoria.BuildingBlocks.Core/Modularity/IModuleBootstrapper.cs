namespace Invoria.BuildingBlocks.Core.Modularity
{
    public interface IModuleBootstrapper
    {
        Task Bootstrap(IServiceProvider serviceProvider);
    }
}
