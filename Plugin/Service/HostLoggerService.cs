
using QQBotCodePlugin.QQBot;
using System;
using static PluginDLL.Class1;

public class HostLoggerService : ILoggerService
{
    private readonly Logger logger;

    public HostLoggerService(Logger logger)
    {
        this.logger = logger;
    }

    public void LogMessage(string level, string message)
    {
        switch (level.ToLower())
        {
            case "debug":
                logger.Debug(message);
                break;
            case "info":
                logger.Info(message);
                break;
            case "waring":
                logger.Warning(message);
                break;
            case "error":
                logger.Error(message);
                break;
            default:
                string errorMessage = $"Invalid log level: {level}";
                logger.Error(errorMessage);
                throw new ArgumentOutOfRangeException(nameof(level), level, errorMessage);
        }
    }
}