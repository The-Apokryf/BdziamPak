using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace BdziamPak.Tests;

/// <summary>
/// A test logger implementation for logging test output.
/// </summary>
/// <typeparam name="T">The type for which the logger is created.</typeparam>
public class TestLogger<T> : ILogger<T>, IDisposable
{
    private readonly ITestOutputHelper _outputHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestLogger{T}"/> class.
    /// </summary>
    /// <param name="outputHelper">The output helper for test output.</param>
    public TestLogger(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    /// <summary>
    /// Disposes the logger.
    /// </summary>
    public void Dispose()
    {
    }

    /// <summary>
    /// Begins a logical operation scope.
    /// </summary>
    /// <typeparam name="TState">The type of the state to begin scope for.</typeparam>
    /// <param name="state">The identifier for the scope.</param>
    /// <returns>An IDisposable that ends the logical operation scope on dispose.</returns>
    public IDisposable BeginScope<TState>(TState state)
    {
        return this;
    }

    /// <summary>
    /// Checks if the given log level is enabled.
    /// </summary>
    /// <param name="logLevel">The log level to check.</param>
    /// <returns>True if the log level is enabled; otherwise, false.</returns>
    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    /// <summary>
    /// Writes a log entry.
    /// </summary>
    /// <typeparam name="TState">The type of the state object.</typeparam>
    /// <param name="logLevel">The log level.</param>
    /// <param name="eventId">The event ID.</param>
    /// <param name="state">The state object.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="formatter">The function to create a log message.</param>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
        Func<TState, Exception, string> formatter)
    {
        _outputHelper.WriteLine($"[{logLevel}, {typeof(T).Name}]:{formatter(state, exception)}");
    }
}