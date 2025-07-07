using MCP.POC.AzureDevOps;
using MCP.POC.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

var builder = Host.CreateApplicationBuilder(args);

// Add user secrets configuration
builder.Configuration.AddUserSecrets<Program>();    // Deployed app consumed by Claude is not accessing environment variables // TODO: implement appSettings.json for the tick tick api key

builder.Logging
    .AddFile("logs/MCP-Logs-{Date}.log")    // %LOCALAPPDATA%\AnthropicClaude\app-0.9.3\logs
    .AddConsole(consoleLogOptions =>
    {
        consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace; // Keep AddConsole() with LogToStandardErrorThreshold to redirect logs to stderr; removing it breaks STDIO transport as MCP requires clean stdout for JSON-RPC communication
    });

// Create a logger instance
var loggerFactory = builder.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger<Program>();

var mcpServerBuilder = builder.Services
    .AddMcpServer();

// Tools registered using attributes
mcpServerBuilder
    .WithStdioServerTransport()
    .WithTools<FileManagerTool>()
    .WithTools<MyResources>();

// This tools can be registered with attributes becasue of the dependencies


// TickTick Client manual registration as tool
var tickTickClient = TickTickClientFactory.New(builder.Configuration, logger);

if (tickTickClient != null)
{
    mcpServerBuilder.WithTools([
        McpServerTool.Create(
            ([Description("The required title of the task to create")]string title,
            [Description("The optional ID of the project to add the task to")]string projectId,
            [Description("The optional detailed description or content of the task")]string content,
            [Description("The optional due date for the task in ISO 8601 format (e.g., 2025-05-10T14:00:00Z)")]string dueDate) => tickTickClient.CreateTask(title, projectId, content, dueDate),
            new()
            {
                Name = "ticktick_create_task",
                Description = "Creates a new task in TickTick."
            }),
        McpServerTool.Create(
            () => tickTickClient.ListProjects(),
            new()
            {
                Name = "ticktick_list_project",
                Description = "Get a list of projects from TickTick."
            }),
        McpServerTool.Create(
            () => tickTickClient.ListInboxTasks(),
            new()
            {
                Name = "ticktick_list_inbox_tasks",
                Description = "Get a list of inbox tasks from TickTick."
            }),
        McpServerTool.Create(
            ([Description("The required ID of the project to list the tasks.")]string projectId) => tickTickClient.ListTasks(projectId),
            new()
            {
                Name = "ticktick_get_task_by_project_id",
                Description = "Get a list of tasks by project id in TickTick."
            }),
    ]);
}

// MyResource manual registration as tool
var myResources = MyResourcesFactory.New(builder.Configuration, loggerFactory);

if (myResources != null)
{
    mcpServerBuilder.WithTools([
        McpServerTool.Create(
            () => myResources.ListResources(),
            new()
            {
                Name = ToolNames.ListResources,
                Description = "My resources, including all my knowledge database (my second brain), it also could include third party resources accessibles and useful to me. If I'm referencing something and you can't find elsewhere, should be here..."
            })
    ]);
}

await builder.Build().RunAsync();
