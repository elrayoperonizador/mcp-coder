using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System;

namespace MCP.POC.AzureDevOps;

/// <summary>
/// Extension methods for registering Azure DevOps tools with MCP
/// </summary>
public static class AzureDevOpsClientFactory
{
    /// <summary>
    /// Gets the Azure DevOps PAT from various potential sources in order of preference
    /// </summary>
    /// <param name="configuration">Configuration containing Azure DevOps PAT</param>
    /// <param name="logger">Optional logger for diagnostic information</param>
    /// <returns>PAT if found, otherwise null</returns>
    private static string GetAzureDevOpsPAT(IConfiguration configuration, ILogger? logger = null)
    {
        // Try to get PAT from configuration (user secrets)
        var pat = configuration["AzureDevOps:PAT"];
        if (!string.IsNullOrWhiteSpace(pat))
        {
            logger?.LogDebug("Azure DevOps PAT found in configuration with key 'AzureDevOps:PAT'");
            return pat;
        }
        
        // If not found in configuration, try legacy format for backward compatibility
        pat = configuration["AzureDevOps.PAT"];
        if (!string.IsNullOrWhiteSpace(pat))
        {
            logger?.LogDebug("Azure DevOps PAT found in configuration with legacy key 'AzureDevOps.PAT'");
            return pat;
        }
        
        // If still not found, try environment variable
        pat = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT");
        if (!string.IsNullOrWhiteSpace(pat))
        {
            logger?.LogDebug("Azure DevOps PAT found in environment variable 'AZURE_DEVOPS_PAT'");
            return pat;
        }
        
        logger?.LogWarning("Azure DevOps PAT not found in any configuration source");
        return null;
    }
    
    /// <summary>
    /// Gets the Azure DevOps organization from various potential sources in order of preference
    /// </summary>
    /// <param name="configuration">Configuration containing Azure DevOps organization</param>
    /// <param name="logger">Optional logger for diagnostic information</param>
    /// <returns>Organization if found, otherwise null</returns>
    private static string GetAzureDevOpsOrganization(IConfiguration configuration, ILogger? logger = null)
    {
        // Try to get organization from configuration (user secrets)
        var org = configuration["AzureDevOps:Organization"];
        if (!string.IsNullOrWhiteSpace(org))
        {
            logger?.LogDebug("Azure DevOps organization found in configuration with key 'AzureDevOps:Organization'");
            return org;
        }
        
        // If not found in configuration, try legacy format for backward compatibility
        org = configuration["AzureDevOps.Organization"];
        if (!string.IsNullOrWhiteSpace(org))
        {
            logger?.LogDebug("Azure DevOps organization found in configuration with legacy key 'AzureDevOps.Organization'");
            return org;
        }
        
        // If still not found, try environment variable
        org = Environment.GetEnvironmentVariable("AZURE_DEVOPS_ORGANIZATION");
        if (!string.IsNullOrWhiteSpace(org))
        {
            logger?.LogDebug("Azure DevOps organization found in environment variable 'AZURE_DEVOPS_ORGANIZATION'");
            return org;
        }
        
        logger?.LogWarning("Azure DevOps organization not found in any configuration source");
        return null;
    }
    
    /// <summary>
    /// Gets the Azure DevOps project from various potential sources in order of preference
    /// </summary>
    /// <param name="configuration">Configuration containing Azure DevOps project</param>
    /// <param name="logger">Optional logger for diagnostic information</param>
    /// <returns>Project if found, otherwise null</returns>
    private static string GetAzureDevOpsProject(IConfiguration configuration, ILogger? logger = null)
    {
        // Try to get project from configuration (user secrets)
        var project = configuration["AzureDevOps:Project"];
        if (!string.IsNullOrWhiteSpace(project))
        {
            logger?.LogDebug("Azure DevOps project found in configuration with key 'AzureDevOps:Project'");
            return project;
        }
        
        // If not found in configuration, try legacy format for backward compatibility
        project = configuration["AzureDevOps.Project"];
        if (!string.IsNullOrWhiteSpace(project))
        {
            logger?.LogDebug("Azure DevOps project found in configuration with legacy key 'AzureDevOps.Project'");
            return project;
        }
        
        // If still not found, try environment variable
        project = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PROJECT");
        if (!string.IsNullOrWhiteSpace(project))
        {
            logger?.LogDebug("Azure DevOps project found in environment variable 'AZURE_DEVOPS_PROJECT'");
            return project;
        }
        
        logger?.LogWarning("Azure DevOps project not found in any configuration source");
        return null;
    }

    public static AzureDevOpsClient New(IConfiguration configuration, ILogger logger = null)
    {
        try
        {
            logger?.LogInformation("Creating DevOpsClient instance from configuration sources");

            // Get configuration parameters
            string project = GetAzureDevOpsProject(configuration, logger);
            string organization = GetAzureDevOpsOrganization(configuration, logger);
            string personalAccessToken = GetAzureDevOpsPAT(configuration, logger);

            var client = new AzureDevOpsClient(personalAccessToken, project, organization);

            logger?.LogInformation("DevOpsClient instance created successfully");

            return client;
        }
        catch (Exception oEx)
        {
            logger?.LogError(oEx, oEx.Message);
            return null;
        }

    }
}
