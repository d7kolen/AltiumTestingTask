using System;
using Microsoft.Extensions.Configuration;

namespace Altium.Core.Infrastructure;

public class ConfigLoader
{
    public IConfigurationRoot Load()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();
    }
}