using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MCP.POC.Tools;

// TODO: dotnet commands not working in the same way as in the command line, will revisit later

/// <summary>
/// Tool for compiling C# code and executing dotnet commands
/// </summary>
[McpServerToolType]
public class DotNetCompilerTool
{
    [McpServerTool(Name = "execute_dotnet_command")]
    [Description("Executes a dotnet CLI command. You should set folders or files in the command as needed.")]
    public static string ExecuteDotnetCommand(string command, string workingDirectory)
    {
        workingDirectory ??= Directory.GetCurrentDirectory();

        return FileSystemHelper.SafeExecute(() =>
        {
            // Validate command does not contain potential security risks
            if (command.Contains("&") || command.Contains("|") || command.Contains(">") ||
                command.Contains("<") || command.Contains(";") || command.Contains("$("))
            {
                return FileSystemHelper.CreateErrorResponse("Command contains invalid characters");
            }

            // Execute the command
            var result = ExecuteDotNetCommand(command, workingDirectory);

            return JsonSerializer.Serialize(new
            {
                success = result.ExitCode == 0,
                output = result.Output,
                exitCode = result.ExitCode
            }, FileSystemHelper.DefaultJsonOptions);
        });
    }

    [McpServerTool(Name = "execute_dotnet_restore_command")]
    [Description("Executes a dotnet restore command on the specified working directory.")]
    public static string ExecuteDotnetRestoreCommand(string workingDirectory)
    {
        // Change to MSBUILD or other mechanism, look at https://chatgpt.com/c/68124c45-87f8-800f-898c-8a553b56a528

        workingDirectory ??= Directory.GetCurrentDirectory();

        return FileSystemHelper.SafeExecute(() =>
        {
            // Execute the command
            var result = ExecuteDotNetCommand("restore", workingDirectory); //"restore -v diag"

            File.WriteAllText(Path.Combine(workingDirectory,"restore_output.log"), result.Output);

            return JsonSerializer.Serialize(new
            {
                success = result.ExitCode == 0,
                output = result.Output,
                exitCode = result.ExitCode
            }, FileSystemHelper.DefaultJsonOptions);
        });
    }

    // Helper method to execute dotnet commands
    private static (int ExitCode, string Output) ExecuteDotNetCommand(string arguments, string workingDirectory)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,                 // e.g. "restore -v diag"
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,   // needed to edit env vars
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };

        var sb = new StringBuilder();
        using var p = Process.Start(psi);
        p.OutputDataReceived += (_, e) => { if (e.Data != null) sb.AppendLine(e.Data); };
        p.ErrorDataReceived += (_, e) => { if (e.Data != null) sb.AppendLine(e.Data); };
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();
        p.WaitForExit();
        return (p.ExitCode, sb.ToString());
    }
}
