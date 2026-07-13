namespace Invoria.BackgroundJob.Core.Jobs;

public interface IJob<in T>
{
    Task Execute(T arg, CancellationToken cancellationToken = default);
}
