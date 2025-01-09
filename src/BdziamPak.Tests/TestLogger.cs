using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace BdziamPak.Tests;

public class TestLogger<T> : ILogger<T>, IDisposable
{
    private readonly ITestOutputHelper _outputHelper;

    public TestLogger(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    public void Dispose()
    {
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return this;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
        Func<TState, Exception, string> formatter)
    {
        _outputHelper.WriteLine($"[{logLevel}, {typeof(T).Name}]:{formatter(state, exception)}");
    }
}