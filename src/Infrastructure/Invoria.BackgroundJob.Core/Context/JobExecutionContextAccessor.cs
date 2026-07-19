namespace Invoria.BackgroundJob.Core.Context;

public class JobExecutionContextAccessor
    : IJobExecutionContextAccessor
{
    private static readonly AsyncLocal<ContextHolder> _current = new();

    public IJobExecutionContext? Current
    {
        get => _current.Value?.Context;

        internal set
        {
            if (_current.Value is not null)
            {
                _current.Value.Context = null;
            }

            if (value is not null)
            {
                _current.Value = new ContextHolder
                {
                    Context = value
                };
            }
        }
    }

    public void SetCurrent(IJobExecutionContext context)
    {
        Current = context;
    }

    public void Clear()
    {
        Current = null;
    }

    private class ContextHolder
    {
        public IJobExecutionContext? Context;
    }
}
