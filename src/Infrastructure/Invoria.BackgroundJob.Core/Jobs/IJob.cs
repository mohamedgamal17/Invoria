namespace Invoria.BackgroundJob.Core.Jobs;

public interface IJob
{
    Task Execute(CancellationToken cancellationToken = default);
}
