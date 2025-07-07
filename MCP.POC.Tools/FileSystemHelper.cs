using System.Text.Json;

namespace MCP.POC.Tools;

/// <summary>
/// Helper class for file system operations with standardized response formatting
/// </summary>
internal static class FileSystemHelper
{
    /// <summary>
    /// Standard options for JSON serialization
    /// </summary>
    public static readonly JsonSerializerOptions DefaultJsonOptions = new()
    {
        WriteIndented = false
    };

    /// <summary>
    /// Creates a success response with a message
    /// </summary>
    public static string CreateSuccessResponse(string message)
    {
        var response = new
        {
            success = true,
            message
        };
        
        return JsonSerializer.Serialize(response, DefaultJsonOptions);
    }

    /// <summary>
    /// Creates a success response with content
    /// </summary>
    public static string CreateSuccessResponse(string message, string content)
    {
        var response = new
        {
            success = true,
            message,
            content
        };
        
        return JsonSerializer.Serialize(response, DefaultJsonOptions);
    }

    /// <summary>
    /// Creates a success response with arbitrary data
    /// </summary>
    public static string CreateSuccessResponse<T>(T data)
    {
        var response = new
        {
            success = true,
            data
        };
        
        return JsonSerializer.Serialize(response, DefaultJsonOptions);
    }

    /// <summary>
    /// Creates an error response
    /// </summary>
    public static string CreateErrorResponse(string error)
    {
        var response = new
        {
            success = false,
            error
        };
        
        return JsonSerializer.Serialize(response, DefaultJsonOptions);
    }

    /// <summary>
    /// Safely executes a file system operation and handles exceptions
    /// </summary>
    public static string SafeExecute(Func<string> operation)
    {
        try
        {
            return operation();
        }
        catch (Exception ex)
        {
            return CreateErrorResponse(ex.Message);
        }
    }
}
