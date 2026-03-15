using Invoria.BuildingBlocks.Infrastructure.Results;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.BuildingBlocks.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationInfrastructure(this IServiceCollection services)
    {
        return services.AddSingleton<IResultToHttpMapper, DefaultResultToHttpMapper>();
    }
}
