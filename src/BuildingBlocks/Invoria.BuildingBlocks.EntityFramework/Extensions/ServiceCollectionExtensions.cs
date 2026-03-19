using Invoria.BuildingBlocks.EntityFramework.Contexts;
using Invoria.BuildingBlocks.EntityFramework.Hooks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.BuildingBlocks.EntityFramework.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInvoriaDbContext<TContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configure)
        where TContext : InvoriaDbContext<TContext>
    {
        ArgumentNullException.ThrowIfNull(configure);


        if(services.SingleOrDefault(x=> x.ServiceType == typeof(IDbHookEngine) ) == null)
        {
            services.AddScoped<IDbHookEngine, DbHookEngine>();

        }


        if (services.SingleOrDefault(x => x.ServiceType == typeof(IBeforeDbHookSave)) == null)
        {
            services.AddScoped<IBeforeDbHookSave, AuditAndIdBeforeSaveHook>();

        }   

        services.AddDbContext<TContext>((sp, options) =>
        {
            configure(options);
        });

        return services;
    }
}

