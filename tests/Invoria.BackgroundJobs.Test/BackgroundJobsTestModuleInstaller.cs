using Invoria.BackgroundJob.Core;
using Invoria.BackgroundJob.Core.Checkpoints;
using Invoria.BackgroundJob.Core.Context;
using Invoria.BackgroundJobs.Test.Fakes;
using Invoria.BuildingBlocks.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.BackgroundJobs.Test;

public class BackgroundJobsTestModuleInstaller : IModuleInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<FakeJobScheduler>();
        services.AddSingleton<IJobScheduler>(sp => sp.GetRequiredService<FakeJobScheduler>());

        services.AddSingleton<FakeRecurringJobScheduler>();
        services.AddSingleton<IRecurringJobScheduler>(sp => sp.GetRequiredService<FakeRecurringJobScheduler>());

        services.AddSingleton<FakeJobCheckpointStore>();
        services.AddSingleton<IJobCheckpointStore>(sp => sp.GetRequiredService<FakeJobCheckpointStore>());

        services.AddSingleton<JobExecutionContextAccessor>();
        services.AddSingleton<IJobExecutionContextAccessor>(
            sp => sp.GetRequiredService<JobExecutionContextAccessor>());
    }
}
