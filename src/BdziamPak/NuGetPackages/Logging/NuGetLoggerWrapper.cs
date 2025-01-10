using Microsoft.Extensions.Logging;
using NuGet.Common;
using ILogger = NuGet.Common.ILogger;
using LogLevel = NuGet.Common.LogLevel;

namespace BdziamPak.NuGetPackages.Logging;

/// <summary>
/// A wrapper for the Microsoft.Extensions.Logging.ILogger to adapt it for use with NuGet's ILogger.
/// </summary>
/// <param name="logger">The Microsoft.Extensions.Logging.ILogger instance to wrap.</param>
public class NuGetLoggerWrapper(Microsoft.Extensions.Logging.ILogger logger) : ILogger
{
    /// <summary>
    /// Logs a debug message.
    /// </summary>
    /// <param name="data">The message to log.</param>
    public void LogDebug(string data)
    {
        logger.LogDebug("[Nugget Debug]:{data}", data);
    }

    /// <summary>
    /// Logs a verbose message.
    /// </summary>
    /// <param name="data">The message to log.</param>
    public void LogVerbose(string data)
    {
        logger.LogTrace("[Nugget Verbose]:{data}", data);
    }

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="data">The message to log.</param>
    public void LogInformation(string data)
    {
        logger.LogDebug("[Nugget Info]:{data}", data);
    }

    /// <summary>
    /// Logs a minimal message.
    /// </summary>
    /// <param name="data">The message to log.</param>
    public void LogMinimal(string data)
    {
        logger.LogDebug("[Nugget Minimal]:{data}", data);
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="data">The message to log.</param>
    public void LogWarning(string data)
    {
        logger.LogDebug("[Nugget Warning]:{data}", data);
    }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="data">The message to log.</param>
    public void LogError(string data)
    {
        logger.LogWarning("[Nugget Error]:{data}", data);
    }

    /// <summary>
    /// Logs an informational summary message.
    /// </summary>
    /// <param name="data">The message to log.</param>
    public void LogInformationSummary(string data)
    {
        logger.LogDebug("[Nugget Info Summary]:{data}", data);
    }

    /// <summary>
    /// Logs a message with the specified log level.
    /// </summary>
    /// <param name="level">The log level.</param>
    /// <param name="data">The message to log.</param>
    public void Log(LogLevel level, string data)
    {
        switch (level)
        {
            case LogLevel.Debug:
                logger.LogDebug(data);
                break;
            case LogLevel.Verbose:
                logger.LogTrace(data);
                break;
            case LogLevel.Information:
                logger.LogInformation(data);
                break;
            case LogLevel.Minimal:
                logger.LogInformation(data);
                break;
            case LogLevel.Warning:
                logger.LogWarning(data);
                break;
            case LogLevel.Error:
                logger.LogError(data);
                break;
            default:
                logger.LogInformation(data);
                break;
        }
    }

    /// <summary>
    /// Asynchronously logs a message with the specified log level.
    /// </summary>
    /// <param name="level">The log level.</param>
    /// <param name="data">The message to log.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task LogAsync(LogLevel level, string data)
    {
        Log(level, data);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Logs a message.
    /// </summary>
    /// <param name="message">The log message.</param>
    public void Log(ILogMessage message)
    {
        Log(message.Level, message.Message);
    }

    /// <summary>
    /// Asynchronously logs a message.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task LogAsync(ILogMessage message)
    {
        Log(message.Level, message.Message);
        return Task.CompletedTask;
    }
}