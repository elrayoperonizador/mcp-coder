using ModelContextProtocol.Server;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MCP.POC.Tools;

public class TickTickClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://api.ticktick.com/open/v1";
    private readonly string _apiKey;

    /// <summary>
    /// Initialize the TickTick tool with an API key
    /// </summary>
    /// <param name="apiKey">TickTick API key for authentication</param>
    public TickTickClient(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));

        _apiKey = apiKey;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    /// <summary>
    /// Lists tasks from TickTick by projectId
    /// </summary>
    public async Task<string> ListTasks(string projectId)
    {
        return await SafeExecuteAsync(async () =>
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/project/{projectId}/data");
            if (!response.IsSuccessStatusCode)
            {
                return CreateErrorResponse($"Error fetching tasks: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            return CreateSuccessResponse("Tasks list retrieved successfully", content);
        });
    }

    /// <summary>
    /// Lists inbox tasks from TickTick
    /// </summary>
    public async Task<string> ListInboxTasks()
    {
        return await SafeExecuteAsync(async () =>
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/project/inbox/data");
            if (!response.IsSuccessStatusCode)
            {
                return CreateErrorResponse($"Error fetching tasks: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            return CreateSuccessResponse("Inbox task list retrieved successfully", content);
        });
    }

    /// <summary>
    /// Gets a specific task from TickTick by ID
    /// </summary>
    public async Task<string> GetTask(string taskId)
    {
        return await SafeExecuteAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(taskId))
            {
                return CreateErrorResponse("Task ID is required");
            }

            var response = await _httpClient.GetAsync($"{_baseUrl}/task/{taskId}");
            if (!response.IsSuccessStatusCode)
            {
                return CreateErrorResponse($"Error fetching task: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            return CreateSuccessResponse("Task retrieved successfully", content);
        });
    }

    /// <summary>
    /// Creates a new task in TickTick
    /// </summary>
    public async Task<string> CreateTask(string title, string projectId = null, string content = null, string dueDate = null)
    {
        return await SafeExecuteAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return CreateErrorResponse("Task title is required");
            }

            // Create task data
            var taskData = new
            {
                title = title,
                content = content,
                projectId = projectId,
                dueDate = dueDate
            };

            var json = JsonSerializer.Serialize(taskData);
            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/task", requestContent);
            if (!response.IsSuccessStatusCode)
            {
                return CreateErrorResponse($"Error creating task: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return CreateSuccessResponse("Task created successfully", responseContent);
        });
    }

    /// <summary>
    /// Gets projects from TickTick
    /// </summary>
    public async Task<string> ListProjects()
    {
        return await SafeExecuteAsync(async () =>
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/project");
            if (!response.IsSuccessStatusCode)
            {
                return CreateErrorResponse($"Error fetching projects: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            return CreateSuccessResponse("Projects retrieved successfully", content);
        });
    }

    /// <summary>
    /// Updates a task in TickTick
    /// </summary>
    public async Task<string> UpdateTask(string taskId, string title = null, string content = null, string status = null, string dueDate = null)
    {
        return await SafeExecuteAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(taskId))
            {
                return CreateErrorResponse("Task ID is required");
            }

            // Create update data (only include fields that are provided)
            var updateData = new Dictionary<string, object>();
            
            if (!string.IsNullOrWhiteSpace(title))
                updateData["title"] = title;
                
            if (!string.IsNullOrWhiteSpace(content))
                updateData["content"] = content;
                
            if (!string.IsNullOrWhiteSpace(status))
                updateData["status"] = status;
                
            if (!string.IsNullOrWhiteSpace(dueDate))
                updateData["dueDate"] = dueDate;

            var json = JsonSerializer.Serialize(updateData);
            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_baseUrl}/task/{taskId}", requestContent);
            if (!response.IsSuccessStatusCode)
            {
                return CreateErrorResponse($"Error updating task: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return CreateSuccessResponse("Task updated successfully", responseContent);
        });
    }

    /// <summary>
    /// Create batch tasks in TickTick
    /// </summary>
    public async Task<string> BatchCreateTasks(string[] tasks)
    {
        return await SafeExecuteAsync(async () =>
        {
            if (tasks == null || tasks.Length == 0)
            {
                return CreateErrorResponse("No tasks provided");
            }

            // Parse tasks into task objects or simple titles
            var tasksList = new List<object>();
            foreach (var task in tasks)
            {
                if (string.IsNullOrWhiteSpace(task))
                {
                    continue; // Skip empty tasks
                }

                try
                {
                    // Try to parse as JSON
                    var taskObj = JsonSerializer.Deserialize<Dictionary<string, object>>(task);
                    tasksList.Add(taskObj);
                }
                catch
                {
                    // If task is just a string, assume it's a title
                    tasksList.Add(new { title = task });
                }
            }

            if (tasksList.Count == 0)
            {
                return CreateErrorResponse("No valid tasks provided");
            }

            var batchData = new { add = tasksList };
            var json = JsonSerializer.Serialize(batchData);
            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/batch/task", requestContent);
            if (!response.IsSuccessStatusCode)
            {
                return CreateErrorResponse($"Error creating batch tasks: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return CreateSuccessResponse("Batch tasks created successfully", responseContent);
        });
    }

    /// <summary>
    /// Utility method to execute API calls safely and handle exceptions
    /// </summary>
    private async Task<string> SafeExecuteAsync(Func<Task<string>> operation)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex)
        {
            return CreateErrorResponse(ex.Message);
        }
    }

    /// <summary>
    /// Creates a standardized error response
    /// </summary>
    private static string CreateErrorResponse(string error)
    {
        var response = new
        {
            success = false,
            error
        };
        
        return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = false });
    }

    /// <summary>
    /// Creates a standardized success response with content
    /// </summary>
    private static string CreateSuccessResponse(string message, string content)
    {
        var response = new
        {
            success = true,
            message,
            content
        };
        
        return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = false });
    }
}
