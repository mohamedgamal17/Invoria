using Invoria.BuildingBlocks.EntityFramework.Contexts;
using Invoria.BuildingBlocks.EntityFramework.Hooks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.BuildingBlocks.EntityFramework.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInvertoDbContext<TContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configure)
        where TContext : InvoriaDbContext
    {
        ArgumentNullException.ThrowIfNull(configure);

        services.AddScoped<IDbHookEngine, DbHookEngine>();
        services.AddScoped<IBeforeDbHookSave, AuditAndIdBeforeSaveHook>();

        services.AddDbContext<TContext>((sp, options) =>
        {
            configure(options);
        });

        return services;
    }
}

