using Serilog;
using Serilog.Core;
using System;

namespace Altium.Core;

public class LoggerFactory
{
    public Logger CreateLogger(string logFolder)
    {
        var config = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                $@"{logFolder}\log-{DateTime.UtcNow:yyyy-MM-dd_hh-mm-ss}.txt",
                rollingInterval: RollingInterval.Infinite)
             .WriteTo.Console();

        return config.CreateLogger();
    }
}