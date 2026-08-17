using Invoria.Application.Tests;
using Invoria.BackgroundJob.Core;
using Invoria.BackgroundJob.Core.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.BackgroundJobs.Test;

public abstract class BackgroundJobTestFixture : TestFixture
{
    protected override Task BeforeAnyTestRunAsync()
    {
        var accessor = ServiceProvider.GetRequiredService<JobExecutionContextAccessor>();

        var jobId = new JobId(GetType().Assembly.GetName().Name!);

        accessor.SetCurrent(new TestJobExecutionContext(jobId, CancellationToken.None));

        return Task.CompletedTask;
    }

    protected override Task AfterAnyTestTearDown()
    {
        var accessor = ServiceProvider.GetRequiredService<JobExecutionContextAccessor>();

        accessor.Clear();

        return Task.CompletedTask;
    }

    private sealed class TestJobExecutionContext
        : IJobExecutionContext
    {
        public JobId JobId { get; }

        public CancellationToken CancellationToken { get; }

        public TestJobExecutionContext(
            JobId jobId,
            CancellationToken cancellationToken)
        {
            JobId = jobId;
            CancellationToken = cancellationToken;
        }
    }
}
