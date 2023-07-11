﻿using Serilog;
using Serilog.Formatting.Json;

namespace TinkoffPriceMonitor.Logs
{
    public static class LoggerInitializer
    {
        public static void InitializeLogger()
        {
            string logFilePath = "log.txt";

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(new JsonFormatter(), logFilePath, rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }
    }
}