using Microsoft.Extensions.Logging;
using NuGet.Common;
using ILogger = NuGet.Common.ILogger;
using LogLevel = NuGet.Common.LogLevel;

namespace BdziamPak.NuGetPackages.Logging;

public class NuGetLoggerWrapper(Microsoft.Extensions.Logging.ILogger logger) : ILogger
{
    public void LogDebug(string data) => logger.LogDebug("[Nugget Debug]:{data}", data);

    public void LogVerbose(string data) => logger.LogTrace("[Nugget Verbose]:{data}", data);

    public void LogInformation(string data) =>logger.LogDebug("[Nugget Info]:{data}", data);

    public void LogMinimal(string data) =>logger.LogDebug("[Nugget Minimal]:{data}", data);

    public void LogWarning(string data) => logger.LogDebug("[Nugget Warning]:{data}", data);

    public void LogError(string data) =>logger.LogWarning("[Nugget Error]:{data}", data);

    public void LogInformationSummary(string data) => logger.LogDebug("[Nugget Info Summary]:{data}", data);
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

    Task ILogger.LogAsync(LogLevel level, string data)
    {
        return LogAsync(level, data);
    }

    public Task LogAsync(LogLevel level, string data)
    {
        Log(level, data);
        return Task.CompletedTask;
    }

    public void Log(ILogMessage message)
    {
        Log(message.Level, message.Message);
    }

    public Task LogAsync(ILogMessage message)
    {
        Log(message.Level, message.Message);
        return Task.CompletedTask;
    }
}