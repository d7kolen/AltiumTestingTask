using System;
using Serilog;
using Serilog.Core;

namespace Altium.Core.Infrastructure;

public class LoggerFactory
{
    public Logger CreateLogger(string logFolder)
    {
        var config = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                $@"{logFolder}\log-{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}.txt",
                rollingInterval: RollingInterval.Infinite)
             .WriteTo.Console();

        return config.CreateLogger();
    }
}