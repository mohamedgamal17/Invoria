using Invoria.BackgroundJob.Core.Context;

namespace Invoria.BackgroundJob.Core.Middlewares;

public delegate Task JobExecutionDelegate(
    IJobExecutionContext context);
