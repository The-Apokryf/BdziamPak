namespace BdziamPak.Tests;

using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

public class TestLogger<T> : ILogger<T>, IDisposable
{
    private readonly ITestOutputHelper _outputHelper;

    public TestLogger(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    public IDisposable BeginScope<TState>(TState state) => this;

    public void Dispose() { }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        _outputHelper.WriteLine($"[{logLevel}, {typeof(T).Name}]:{formatter(state, exception)}");
    }
}
