using System.Reflection;
using Invoria.BackgroundJob.Core.Context;
using Invoria.BackgroundJob.Core.Jobs;
using Invoria.BackgroundJob.Core.Middlewares;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.BackgroundJob.Core;

public static class BackgroundJobsServiceCollectionExtensions
{
    public static IBackgroundJobsBuilder AddBackgroundJobs(
        this IServiceCollection services)
    {
        services.AddSingleton<JobExecutionContextAccessor>();
        services.AddSingleton<IJobExecutionContextAccessor>(
            sp => sp.GetRequiredService<JobExecutionContextAccessor>());

        // TODO: Register IJobMiddlewarePipeline once the pipeline implementation is created.

        return new BackgroundJobsBuilder(services);
    }

    public static IBackgroundJobsBuilder UseLoggingJobMiddleware(
        this IBackgroundJobsBuilder builder)
    {
        builder.Services.AddSingleton<IJobMiddleware, LoggingJobMiddleware>();

        return builder;
    }

    public static IBackgroundJobsBuilder AddJob<TJob>(
        this IBackgroundJobsBuilder builder,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TJob : class, IJob
    {
        builder.Services.Add(new ServiceDescriptor(typeof(IJob), typeof(TJob), lifetime));

        return builder;
    }

    public static IBackgroundJobsBuilder AddJob(
        this IBackgroundJobsBuilder builder,
        Type jobType,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        if (!typeof(IJob).IsAssignableFrom(jobType))
        {
            throw new ArgumentException($"'{jobType.Name}' must implement IJob.", nameof(jobType));
        }

        builder.Services.Add(new ServiceDescriptor(typeof(IJob), jobType, lifetime));

        return builder;
    }

    public static IBackgroundJobsBuilder AddJobsFromAssembly(
        this IBackgroundJobsBuilder builder,
        Assembly assembly,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        var jobTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IJob).IsAssignableFrom(t))
            .ToList();

        foreach (var type in jobTypes)
        {
            builder.Services.Add(new ServiceDescriptor(typeof(IJob), type, lifetime));
        }

        return builder;
    }

    public static IBackgroundJobsBuilder AddJobsFromAssemblyContaining<T>(
        this IBackgroundJobsBuilder builder,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        return builder.AddJobsFromAssembly(typeof(T).Assembly, lifetime);
    }
}

