using ModelContextProtocol.Server;
using System.ComponentModel;
using Microsoft.Extensions.AI;

namespace MCP.POC.Tools;

/// <summary>
/// Prompts for the MCP server (they are ignored by Claude, can be used by a manual client, check https://github.com/modelcontextprotocol/csharp-sdk).
/// </summary>
[McpServerPromptType]
public static class CustomPrompts
{
    [McpServerPrompt, Description("Creates a custom prompt that should be used for code related tasks.")]
    public static ChatMessage CustomCodingRelatedPrompt(
        [Description("The languge to use")] string language,
        [Description("The original user prompt")] string originalPrompt) =>
        new(ChatRole.User, $"Please replay to the following code related question in the {language} language: {originalPrompt}" + Environment.NewLine +
                "# Additional instructions:" + Environment.NewLine +
                "- You can use any of your available tools, if any error ocurr, you will ask the user before trying to execute some additional tool to fix the issue.");
}
