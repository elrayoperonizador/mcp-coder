# mcp-coder

A standalone console-based Model Context Protocol (MCP) server providing comprehensive tools for coding tasks, file management, and productivity integrations.

## Overview

This MCP server exposes a rich set of tools designed to assist with software development and project management tasks. It integrates with various services and provides a standardized interface for AI assistants like Claude to interact with your development environment.

## Features

### 🗂️ File Management Tools
- **Create/Update/Delete Files**: Full CRUD operations on files and directories
- **Directory Exploration**: List files and folder structures recursively
- **Content Search**: Find and read files matching DOS-style patterns
- **Batch Operations**: Process multiple files efficiently

### 📚 Resource Management
- **Knowledge Base Integration**: Access to your "second brain" resources
- **Resource Discovery**: Automatic listing of configured knowledge resources
- **Flexible Resource Types**: Support for various file types and external resources

### ✅ TickTick Integration
- **Task Management**: Create, update, and list tasks
- **Project Organization**: Manage projects and organize tasks
- **Batch Operations**: Create multiple tasks at once
- **Inbox Management**: Access and manage your inbox tasks

### 🔧 Azure DevOps Integration (Optional)
- Integration with Azure DevOps services for enterprise environments

## Project Structure

```
mcp-coder/
├── MCP.POC.Console.Server/     # Main console application
├── MCP.POC.Tools/              # Core MCP tools implementation
├── MCP.POC.AzureDevOps/        # Azure DevOps integration
└── README.md                   # This file
```

## Prerequisites

- .NET 9.0 SDK
- Visual Studio 2022 or Visual Studio Code
- TickTick API key (optional, for task management features)
- Azure DevOps credentials (optional, for Azure DevOps integration)

## Installation & Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd mcp-coder
   ```

2. **Build the solution**
   ```bash
   dotnet build
   ```

3. **Configure User Secrets** (for TickTick integration)
   ```bash
   cd MCP.POC.Console.Server
   dotnet user-secrets init
   dotnet user-secrets set "TickTick:ApiKey" "your-ticktick-api-key"
   ```

4. **Configure Resources** (optional)
   
   Add resource configuration to your user secrets or appsettings:
   ```json
   {
     "MyResources": {
       "Resource1": {
         "Name": "My Knowledge Base",
         "FullPath": "C:\\path\\to\\your\\knowledge\\base",
         "Description": "Personal knowledge database",
         "PreferredTool": "markdown"
       }
     }
   }
   ```

5. **Configure Claude Desktop**
  Edit your claude_desktop_config.json file and add the location of your generated executable.
  Close and restart Claude from your system tray
  ```
  {
    "mcpServers": {
      "mcp-coder": {
        "command": "D:\\Learn\\mcp-coder\\MCP.POC.Console.Server\\bin\\Debug\\net9.0\\MCP.POC.Console.Server.exe",
        "args": []
      }
    }
  }
  ```

## Available Tools

### File Management
- `create_file` - Creates a new file with specified content
- `update_file` - Updates an existing file with new content
- `delete_file` - Deletes a file
- `create_directory` - Creates a new directory
- `return_filenames` - Lists all files in a directory and subdirectories
- `return_folder_structure` - Lists all folders in a directory and subdirectories
- `return_content_from_filtered_files_in_folder` - Gets content from files matching patterns

### Resource Management
- `list_resources` - Lists all configured knowledge resources

### TickTick Integration (when configured)
- `ticktick_create_task` - Creates a new task
- `ticktick_list_project` - Lists all projects
- `ticktick_list_inbox_tasks` - Lists inbox tasks
- `ticktick_get_task_by_project_id` - Gets tasks for a specific project

## Configuration

### User Secrets
Store sensitive configuration like API keys using .NET User Secrets:

```bash
dotnet user-secrets set "TickTick:ApiKey" "your-api-key-here"
```

### appsettings.json (Alternative)
```json
{
  "TickTick": {
    "ApiKey": "your-ticktick-api-key"
  },
  "MyResources": {
    "KnowledgeBase": {
      "Name": "Documentation",
      "FullPath": "C:\\docs",
      "Description": "Project documentation",
      "PreferredTool": "markdown"
    }
  }
}
```

## Usage Examples

### Typical Workflow

1. **Explore Project Structure**
   ```
   Use `return_folder_structure` to understand the project layout
   Use `return_filenames` to see all files
   ```

2. **Read Existing Code**
   ```
   Use `return_content_from_filtered_files_in_folder` with patterns like "*.cs", "*.js"
   ```

3. **Create/Modify Files**
   ```
   Use `create_file` for new files
   Use `update_file` to modify existing files
   ```

4. **Manage Tasks**
   ```
   Use `ticktick_create_task` to track development tasks
   Use `ticktick_list_project` to organize work
   ```

### Sample Prompt for Claude

When using this MCP server with Claude, you can use prompts like:

```
Check the code at D:\Learn\mcp-coder and under that folder update the readme.md file with any information useful for someone interested in using this repo.
Include the prompt as an example
```

This prompt will trigger Claude to:
1. Explore the folder structure using `return_folder_structure`
2. List all files using `return_filenames`
3. Read existing code using `return_content_from_filtered_files_in_folder`
4. Analyze the project structure and functionality
5. Update the README.md file using `update_file`

## Logging

The server uses Serilog for comprehensive logging:
- Console logging for development
- File logging to `logs/MCP-Logs-{Date}.log`
- Configurable log levels

## Development

### Adding New Tools

1. Create a new tool class in `MCP.POC.Tools`
2. Use the `[McpServerToolType]` attribute on the class
3. Use the `[McpServerTool(Name = "tool_name")]` attribute on methods
4. Register the tool in `Program.cs`

### Architecture

- **MCP.POC.Console.Server**: Main entry point and server configuration
- **MCP.POC.Tools**: Core tool implementations
- **MCP.POC.AzureDevOps**: Azure DevOps specific integrations
- Uses dependency injection for configuration and logging
- Supports both attribute-based and manual tool registration

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## Troubleshooting

### Common Issues

1. **Server not starting**: Check that .NET 9.0 is installed
2. **TickTick integration failing**: Verify API key configuration
3. **File operations failing**: Check file permissions and paths
4. **Logging issues**: Ensure write permissions to logs directory

## Related Resources

- [Model Context Protocol Documentation](https://modelcontextprotocol.io/)
- [TickTick API Documentation](https://developer.ticktick.com/)
- [.NET 9.0 Documentation](https://docs.microsoft.com/en-us/dotnet/)
