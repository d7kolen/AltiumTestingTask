using Microsoft.Extensions.Configuration;
using System;

namespace Altium.Core;

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