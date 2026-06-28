using Invoria.BuildingBlocks.EntityFramework.Contexts;
using Invoria.BuildingBlocks.EntityFramework.Hooks;
using Invoria.BuildingBlocks.EntityFramework.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.BuildingBlocks.EntityFramework.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInvoriaDbHooks(this IServiceCollection services)
    {
        services.AddScoped<IDbHookEngine, DbHookEngine>();
        services.AddScoped<IEntityCrudChangeAccumulator, EntityCrudChangeAccumulator>();
        services.AddTransient<IBeforeDbHookSave, AuditAndIdBeforeSaveHook>();
        services.AddScoped<IBeforeDbHookSave, EntityCrudCaptureBeforeSaveHook>();
        services.AddScoped<IAfterDbHookSave, DispatchDomainEventsAfterSaveHook>();
        services.AddScoped<IAfterDbHookSave, DispatchEntityCrudDomainEventsAfterSaveHook>();
        return services;
    }

    public static IServiceCollection AddInvoriaDbContext<TContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configure)
        where TContext : InvoriaDbContext<TContext>
    {
        ArgumentNullException.ThrowIfNull(configure);

        if (!services.Any(sd => sd.ServiceType == typeof(IDbHookEngine)))
        {
            services.AddInvoriaDbHooks();
        }

        services.AddDbContext<TContext>((sp, options) =>
        {
            configure(options);
        });

        return services;
    }

    public static IServiceCollection AddInvoriaUnitOfWork<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        services.AddScoped<EfUnitOfWork<TContext>>();
        return services;
    }
}

