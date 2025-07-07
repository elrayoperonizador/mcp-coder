using ModelContextProtocol.Server;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.ComponentModel;
using System.Text;

namespace MCP.POC.Tools;

/// <summary>
/// Tool for file and directory operations
/// </summary>
[McpServerToolType]
public class FileManagerTool
{
    [McpServerTool(Name = "create_file"),
     Description("Creates a new file with the specified content.")]
    public static string CreateFile(string fullPath, string content)
    {
        return FileSystemHelper.SafeExecute(() =>
        {
            // Ensure directory exists
            string directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Check if file already exists
            if (File.Exists(fullPath))
            {
                return FileSystemHelper.CreateErrorResponse($"File already exists: {fullPath}");
            }

            // Write content to file
            File.WriteAllText(fullPath, content);

            return FileSystemHelper.CreateSuccessResponse($"File created successfully: {fullPath}");
        });
    }

    [McpServerTool(Name = "update_file"),
     Description("Updates an existing file with new content.")]
    public static string UpdateFile(string fullPath, string content)
    {
        return FileSystemHelper.SafeExecute(() =>
        {
            // Check if file exists
            if (!File.Exists(fullPath))
            {
                return FileSystemHelper.CreateErrorResponse($"File does not exist: {fullPath}");
            }

            // Write content to file
            File.WriteAllText(fullPath, content);

            return FileSystemHelper.CreateSuccessResponse($"File updated successfully: {fullPath}");
        });
    }

    [McpServerTool(Name = "delete_file"),
     Description("Deletes a file.")]
    public static string DeleteFile(string fullPath)
    {
        return FileSystemHelper.SafeExecute(() =>
        {
            // Check if file exists
            if (!File.Exists(fullPath))
            {
                return FileSystemHelper.CreateErrorResponse($"File does not exist: {fullPath}");
            }

            // Delete file
            File.Delete(fullPath);

            return FileSystemHelper.CreateSuccessResponse($"File deleted successfully: {fullPath}");
        });
    }

    [McpServerTool(Name = "create_directory"),
     Description("Creates a new directory.")]
    public static string CreateDirectory(string fullPath)
    {
        return FileSystemHelper.SafeExecute(() =>
        {
            // Check if directory already exists
            if (Directory.Exists(fullPath))
            {
                return FileSystemHelper.CreateErrorResponse($"Directory already exists: {fullPath}");
            }

            // Create directory
            Directory.CreateDirectory(fullPath);

            return FileSystemHelper.CreateSuccessResponse($"Directory created successfully: {fullPath}");
        });
    }

    [McpServerTool(Name = "return_filenames"),
     Description("Read all folders (and sub-folders) under the given starting folder and return a JSON array of full file names.")]
    public static string ReadFilenames(string startingPath)
    {
        return FileSystemHelper.SafeExecute(() =>
        {
            if (string.IsNullOrWhiteSpace(startingPath) || !Directory.Exists(startingPath))
            {
                return JsonSerializer.Serialize(Array.Empty<string>(), FileSystemHelper.DefaultJsonOptions);
            }

            // Get all files including those in subdirectories
            var files = Directory.GetFiles(startingPath, "*", SearchOption.AllDirectories);
            
            return JsonSerializer.Serialize(files, FileSystemHelper.DefaultJsonOptions);
        });
    }

    [McpServerTool(Name = "return_folder_structure"),
     Description("Read all folders (and sub-folders) under the given starting folder and return a JSON array of full folder names.")]
    public static string ReadFolderStructure(string startingPath)
    {
        return FileSystemHelper.SafeExecute(() =>
        {
            if (string.IsNullOrWhiteSpace(startingPath) || !Directory.Exists(startingPath))
            {
                return JsonSerializer.Serialize(Array.Empty<string>(), FileSystemHelper.DefaultJsonOptions);
            }

            // Get all directories including subdirectories
            var directories = Directory.GetDirectories(startingPath, "*", SearchOption.AllDirectories);
            
            return JsonSerializer.Serialize(directories, FileSystemHelper.DefaultJsonOptions);
        });
    }

    [McpServerTool(Name = "return_content_from_filtered_files_in_folder"),
     Description("Get all matching files using the DOS-style pattern under the given folder (and sub-folders) and return a JSON array of filename and content.")]
    public static string ReadFilesContent(string folderPath, string[] searchPatterns)
    {
        return FileSystemHelper.SafeExecute(() =>
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath) || searchPatterns.Length == 0)
            {
                return "{}";
            }

            List<string> files = new List<string>();

            foreach (var pattern in searchPatterns)
            {
                files.AddRange(Directory.GetFiles(folderPath, pattern, SearchOption.AllDirectories));
            }

            // Project into anonymous objects
            var list = files.Select(path => new
            {
                filename = Path.GetFullPath(path),
                content = File.ReadAllText(path)
            }).ToList();

            return JsonSerializer.Serialize(list, FileSystemHelper.DefaultJsonOptions);
        });
    }
}
