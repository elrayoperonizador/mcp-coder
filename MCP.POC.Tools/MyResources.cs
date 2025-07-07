using ModelContextProtocol.Server;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.ComponentModel;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace MCP.POC.Tools;

/// <summary>
/// Tool to enumerate my available resources to be used by the client
/// </summary>
public class MyResources
{
    IConfiguration configuration;
    ILogger logger;

    public MyResources(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        this.configuration = configuration;
        logger = loggerFactory.CreateLogger<MyResources>();
    }

    public string ListResources()
    {
        return FileSystemHelper.SafeExecute(() =>
        {
            var myResourcesSection = configuration.GetSection("MyResources");
            var resources = new List<FileResource>();

            foreach (var section in myResourcesSection.GetChildren())
            {
                var resource = new FileResource(
                    section["Name"] ?? string.Empty,
                    section["FullPath"] ?? string.Empty,
                    section["Description"] ?? string.Empty,
                    section["PreferredTool"]
                );
                resources.Add(resource);
            }

            var stringResources = JsonSerializer.Serialize(resources);

            logger.LogInformation(stringResources);

            return stringResources;
        });
    }
}
