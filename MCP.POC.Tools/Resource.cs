namespace MCP.POC.Tools;

public class FileResource
{
    public FileResource(string name, string fullPath, string description, string? preferredTool)
    {
        Name = name;
        FullPath = fullPath;
        Description = description;
        PreferredTool = preferredTool;
    }

    public FileResource(string name, string fullPath, string description) : this(name, fullPath, description, null)
    {
    }

    public string Name { get; set; }
    public string Description { get; set; }
    public string FullPath { get; set; }
    public string? PreferredTool { get; set; }
}
