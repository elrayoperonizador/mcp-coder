using ModelContextProtocol.Server;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.ComponentModel;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading;
using Microsoft.Extensions.AI;

namespace MCP.POC.Tools;

/// <summary>
/// Tool for testing sampling
/// Not supported by Claude yet, removing for now
/// https://modelcontextprotocol.io/docs/concepts/sampling
/// </summary>
[McpServerToolType]
public class WritingTool
{
    [McpServerTool(Name = "summarize"),
     Description("Creates a summary for the specified content.")]
    public async static Task<string> Summarize(IMcpServer mcpServer,
        ILogger<WritingTool> logger,
        string content,
        CancellationToken cancellationToken)
    {
        try
        {
            if (mcpServer == null)
            {
                logger.LogError("MCP server is null");
                return string.Empty;
            }

            if (string.IsNullOrEmpty(content))
            {
                logger.LogError("Content is null or empty");
                return string.Empty;
            }

            ChatMessage[] messages = [
                new(ChatRole.User, "Briefly summarize the following content:"),
            new(ChatRole.User, content)];

            var response = await mcpServer.AsSamplingChatClient().GetResponseAsync(messages, null, cancellationToken);

            return response.Text;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in Summarize");
            return string.Empty;
        }


    }

}
