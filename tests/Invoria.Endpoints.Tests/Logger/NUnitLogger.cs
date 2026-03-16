using Microsoft.Extensions.Logging;
namespace Invoria.Endpoints.Tests.Logger
{
    public class NUnitLogger<T> : ILogger<T>
    {


        public IDisposable BeginScope<TState>(TState state) => null!;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);

            TestContext.Progress.WriteLine($"[{logLevel}] {typeof(T).Name}: {message}");

            if (exception != null)
                TestContext.Progress.WriteLine(exception);
        }
    }
}
