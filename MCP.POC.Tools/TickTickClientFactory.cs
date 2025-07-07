using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System;

namespace MCP.POC.Tools;

/// <summary>
/// Factory for the TickTickClient
/// </summary>
public static class TickTickClientFactory
{
    /// <summary>
    /// Gets the TickTick API key from various potential sources in order of preference
    /// </summary>
    /// <param name="configuration">Configuration containing TickTick API Key</param>
    /// <param name="logger">Optional logger for diagnostic information</param>
    /// <returns>API key if found, otherwise null</returns>
    private static string GetTickTickApiKey(IConfiguration configuration, ILogger? logger = null)
    {
        // Try to get API key from configuration (user secrets)
        var apiKey = configuration["TickTick:ApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            logger?.LogInformation("TickTick API key found in configuration with key 'TickTick:ApiKey'");
            return apiKey;
        }
        
        // If not found in configuration, try legacy format for backward compatibility
        apiKey = configuration["TickTick.ApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            logger?.LogInformation("TickTick API key found in configuration with legacy key 'TickTick.ApiKey'");
            return apiKey;
        }
        
        // If still not found, try environment variable
        apiKey = Environment.GetEnvironmentVariable("TICKTICK_API_KEY");
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            logger?.LogInformation("TickTick API key found in environment variable 'TICKTICK_API_KEY'");
            return apiKey;
        }
        
        logger?.LogWarning("TickTick API key not found in any configuration source");
        return null;
    }
    
    /// <summary>
    /// Validates the API key and throws appropriate exception if invalid
    /// </summary>
    /// <param name="apiKey">The API key to validate</param>
    /// <param name="logger">Optional logger for diagnostic information</param>
    /// <exception cref="InvalidOperationException">Thrown when the API key is missing</exception>
    private static void ValidateApiKey(string apiKey, ILogger logger = null)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger?.LogError("TickTick API key validation failed: API key is null or empty");
            throw new InvalidOperationException(
                "TickTick API key is not configured. Add 'TickTick:ApiKey' to your user secrets using " +
                "'dotnet user-secrets set \"TickTick:ApiKey\" \"your-api-key-here\"' " +
                "or set the TICKTICK_API_KEY environment variable."
            );
        }
        
        logger?.LogInformation("TickTick API key validation successful");
    }
     
    /// <summary>
    /// Creates a TickTickTool instance using configuration or environment variables
    /// </summary>
    /// <param name="configuration">Configuration containing TickTick API Key</param>
    /// <param name="logger">Optional logger for diagnostic information</param>
    /// <returns>A configured TickTickTool instance</returns>
    public static TickTickClient New(IConfiguration configuration, ILogger logger = null)
    {
        try
        {
            logger?.LogInformation("Creating TickTickClient instance from configuration sources");
        
            // Get API key from available sources
            string apiKey = GetTickTickApiKey(configuration, logger);
        
            // Validate the API key
            ValidateApiKey(apiKey, logger);

            var client = new TickTickClient(apiKey);
        
            logger?.LogInformation("TickTickClient instance created successfully");

            return client;
        }
        catch (Exception oEx)
        {
            logger?.LogError(oEx, oEx.Message);
            return null;
        }

    }
}
