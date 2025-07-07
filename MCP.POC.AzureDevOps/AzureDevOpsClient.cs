using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System.Text;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using System.Linq;

namespace MCP.POC.AzureDevOps;

public class AzureDevOpsClient
{
    readonly string _personalAccessToken;
    readonly string _project;
    readonly string _organization;
    readonly string _orgUrl;

    // Configurable field name for BillingReferenceId
    public string BillingReferenceFieldName { get; set; } = "System.Tags"; // Default to using Tags

    public AzureDevOpsClient(string personalAccessToken, string project, string organization)
    {
        _personalAccessToken = personalAccessToken;
        _project = project;
        _organization = organization;
        _orgUrl = _organization.ToLowerInvariant().StartsWith("https://")
            ? _organization
            : $"https://dev.azure.com/{_organization}";
    }

    public Task<IEnumerable<WorkItem>> GetWorkItems(
        string[] workItemTypes = null,
        string[]? states = null,
        int maxResults = 50)
    {
        return QueryWorkItemsAsync(
            _orgUrl,
            _project,
            _personalAccessToken,
            workItemTypes,
            states,
            maxResults);
    }

    public async Task<WorkItem> CreateWorkItem(WorkItem workItem, string workItemType = "Task")
    {
        return await CreateWorkItemAsync(_orgUrl, _project, _personalAccessToken, workItem, workItemType);
    }

    public async Task<WorkItem> GetWorkItem(int workItemId)
    {
        return await GetWorkItemAsync(_orgUrl, _personalAccessToken, workItemId);
    }

    public async Task DeleteWorkItem(int workItemId)
    {
        await DeleteWorkItemAsync(_orgUrl, _personalAccessToken, workItemId);
    }

    public async Task<WorkItem> UpdateWorkItem(WorkItem updatedWorkItem)
    {
        return await UpdateWorkItemAsync(_orgUrl, _personalAccessToken, updatedWorkItem);
    }

    /// <summary>
    /// Sets the BillingReferenceId on a WorkItem using the configured field name
    /// </summary>
    public void SetBillingReferenceId(WorkItem workItem, string billingReferenceId)
    {
        if (workItem.Fields == null)
            workItem.Fields = new Dictionary<string, object>();

        if (BillingReferenceFieldName == "System.Tags")
        {
            // Handle tags specially - append to existing tags
            var existingTags = workItem.Fields.ContainsKey("System.Tags") 
                ? workItem.Fields["System.Tags"]?.ToString() ?? ""
                : "";
            
            var billingTag = $"BillingRef:{billingReferenceId}";
            
            if (string.IsNullOrEmpty(existingTags))
            {
                workItem.Fields["System.Tags"] = billingTag;
            }
            else if (!existingTags.Contains(billingTag))
            {
                workItem.Fields["System.Tags"] = $"{existingTags}; {billingTag}";
            }
        }
        else
        {
            workItem.Fields[BillingReferenceFieldName] = billingReferenceId;
        }
    }

    /// <summary>
    /// Gets the BillingReferenceId from a WorkItem using the configured field name
    /// </summary>
    public string GetBillingReferenceId(WorkItem workItem)
    {
        if (workItem.Fields == null)
            return null;

        if (BillingReferenceFieldName == "System.Tags")
        {
            // Extract from tags
            var tags = workItem.Fields.ContainsKey("System.Tags") 
                ? workItem.Fields["System.Tags"]?.ToString() ?? ""
                : "";
            
            var billingTagPrefix = "BillingRef:";
            var startIndex = tags.IndexOf(billingTagPrefix);
            if (startIndex >= 0)
            {
                startIndex += billingTagPrefix.Length;
                var endIndex = tags.IndexOf(';', startIndex);
                if (endIndex < 0) endIndex = tags.Length;
                return tags.Substring(startIndex, endIndex - startIndex).Trim();
            }
            return null;
        }
        else
        {
            return workItem.Fields.ContainsKey(BillingReferenceFieldName) 
                ? workItem.Fields[BillingReferenceFieldName]?.ToString()
                : null;
        }
    }

    /// <summary>
    /// Returns the web URL for a work item (for browsing in Azure DevOps)
    /// </summary>
    public string GetWorkItemWebUrl(int workItemId)
    {
        // _orgUrl is like https://dev.azure.com/{org}
        // _project is the project name
        // Format: https://dev.azure.com/{org}/{project}/_workitems/edit/{id}
        var org = _organization.TrimEnd('/');
        var project = _project;
        return $"{org}/{project}/_workitems/edit/{workItemId}";
    }

    async Task<WorkItem> GetWorkItemAsync(string orgUrl, string personalAccessToken, int workItemId)
    {
        try
        {
            VssConnection connection = new VssConnection(
                new Uri(orgUrl),
                new VssBasicCredential(string.Empty, personalAccessToken)
            );

            using WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();
            WorkItem workItem = await witClient.GetWorkItemAsync(workItemId);
            return workItem;
        }
        catch (Exception)
        {
            return null;
        }
    }

    async Task DeleteWorkItemAsync(string orgUrl, string personalAccessToken, int workItemId)
    {
        VssConnection connection = new VssConnection(
            new Uri(orgUrl),
            new VssBasicCredential(string.Empty, personalAccessToken)
        );

        using WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();
        await witClient.DeleteWorkItemAsync(workItemId);
    }

    async Task<WorkItem> UpdateWorkItemAsync(string orgUrl, string personalAccessToken, WorkItem updatedWorkItem)
    {
        var workItemId = updatedWorkItem.Id.GetValueOrDefault();

        VssConnection connection = new VssConnection(
            new Uri(orgUrl),
            new VssBasicCredential(string.Empty, personalAccessToken)
        );

        using WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();

        WorkItem existingWorkItem = await witClient.GetWorkItemAsync(workItemId);
        if (existingWorkItem == null)
        {
            throw new ArgumentException($"Work item with ID {workItemId} not found");
        }

        JsonPatchDocument patchDocument = new JsonPatchDocument();

        if (updatedWorkItem.Fields != null)
        {
            foreach (var field in updatedWorkItem.Fields)
            {
                if (IsSystemField(field.Key) && !IsUpdatableSystemField(field.Key))
                {
                    continue;
                }

                if (!existingWorkItem.Fields.ContainsKey(field.Key) || 
                    !AreFieldValuesEqual(existingWorkItem.Fields[field.Key], field.Value))
                {
                    try
                    {
                        patchDocument.Add(
                            new JsonPatchOperation()
                            {
                                Operation = Operation.Replace,
                                Path = $"/fields/{field.Key}",
                                Value = field.Value
                            }
                        );
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Warning: Could not update field {field.Key}: {ex.Message}");
                    }
                }
            }
        }

        if (patchDocument.Count == 0)
        {
            return existingWorkItem;
        }

        WorkItem updatedWorkItemResult = await witClient.UpdateWorkItemAsync(patchDocument, workItemId);
        return updatedWorkItemResult;
    }

    bool IsSystemField(string fieldName)
    {
        return fieldName.StartsWith("System.", StringComparison.OrdinalIgnoreCase) ||
               fieldName.StartsWith("Microsoft.VSTS.", StringComparison.OrdinalIgnoreCase);
    }

    bool IsUpdatableSystemField(string fieldName)
    {
        var updatableSystemFields = new[]
        {
            "System.Title",
            "System.Description",
            "System.State",
            "System.AssignedTo",
            "System.Reason",
            "System.Tags",
            "Microsoft.VSTS.Common.Priority",
            "Microsoft.VSTS.Common.Severity",
            "Microsoft.VSTS.Scheduling.Effort",
            "Microsoft.VSTS.Scheduling.OriginalEstimate",
            "Microsoft.VSTS.Scheduling.RemainingWork",
            "Microsoft.VSTS.Scheduling.CompletedWork"
        };

        return updatableSystemFields.Contains(fieldName, StringComparer.OrdinalIgnoreCase);
    }

    bool AreFieldValuesEqual(object existingValue, object newValue)
    {
        if (existingValue == null && newValue == null) return true;
        if (existingValue == null || newValue == null) return false;

        string existingStr = existingValue.ToString();
        string newStr = newValue.ToString();

        return string.Equals(existingStr, newStr, StringComparison.Ordinal);
    }

    async Task<WorkItem> CreateWorkItemAsync(
        string orgUrl,
        string project,
        string personalAccessToken,
        WorkItem workItem,
        string workItemType)
    {
        VssConnection connection = new VssConnection(
            new Uri(orgUrl),
            new VssBasicCredential(string.Empty, personalAccessToken)
        );

        using WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();

        JsonPatchDocument patchDocument = new JsonPatchDocument();

        // Add required fields first
        string title = workItem.Fields?.ContainsKey("System.Title") == true
            ? workItem.Fields["System.Title"]?.ToString()
            : "New Work Item";

        patchDocument.Add(
            new JsonPatchOperation()
            {
                Operation = Operation.Add,
                Path = "/fields/System.Title",
                Value = title
            }
        );

        // Add all other fields from the work item
        if (workItem.Fields != null)
        {
            foreach (var field in workItem.Fields)
            {
                // Skip title as we already added it
                if (field.Key == "System.Title")
                    continue;

                try
                {
                    patchDocument.Add(
                        new JsonPatchOperation()
                        {
                            Operation = Operation.Add,
                            Path = $"/fields/{field.Key}",
                            Value = field.Value
                        }
                    );
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Warning: Could not add field {field.Key} during creation: {ex.Message}");
                }
            }
        }

        WorkItem createdWorkItem = await witClient.CreateWorkItemAsync(patchDocument, project, workItemType);
        return createdWorkItem;
    }

    async Task<IEnumerable<WorkItem>> QueryWorkItemsAsync(
        string orgUrl,
        string project,
        string personalAccessToken,
        string[]? workItemTypes,
        string[]? states,
        int maxResults = 50)
    {
        VssConnection connection = new VssConnection(
            new Uri(orgUrl),
            new VssBasicCredential(string.Empty, personalAccessToken)
        );

        using WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();

        StringBuilder queryBuilder = new StringBuilder();
        queryBuilder.Append("Select [System.Id], [System.Title], [System.State], [System.WorkItemType], [System.AssignedTo], [System.CreatedDate], [System.ChangedDate] ");
        queryBuilder.Append("From WorkItems ");
        queryBuilder.Append($"Where [System.TeamProject] = '{project}' ");

        if (workItemTypes != null && workItemTypes.Length > 0)
        {
            var escapedTypes = workItemTypes.Select(t => $"'{t.Replace("'", "''")}'");
            queryBuilder.Append($"And [System.WorkItemType] In ({string.Join(", ", escapedTypes)}) ");
        }

        if (states != null && states.Length > 0)
        {
            var escapedStates = states.Select(s => $"'{s.Replace("'", "''")}'");
            queryBuilder.Append($"And [System.State] In ({string.Join(", ", escapedStates)}) ");
        }

        queryBuilder.Append("Order By [System.ChangedDate] Desc");

        Wiql wiql = new Wiql { Query = queryBuilder.ToString() };
        WorkItemQueryResult queryResult = await witClient.QueryByWiqlAsync(wiql);

        if (queryResult.WorkItems.Count() == 0)
        {
            return new WorkItem[] { };
        }

        List<int> ids = queryResult.WorkItems
            .Select(wir => wir.Id)
            .ToList();

        var workItems = new List<WorkItem>();
        const int batchSize = 200;

        for (int i = 0; i < ids.Count; i += batchSize)
        {
            var batchIds = ids.Skip(i).Take(batchSize).ToList();
            var batchWorkItems = await witClient.GetWorkItemsAsync(batchIds);
            workItems.AddRange(batchWorkItems);
        }

        return workItems;
    }
}
