

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Stellaris.Shared.AzureDevOps;
using Stellaris.Shared.Business.Abstractions;
using Stellaris.Shared.Config;
using Stellaris.Shared.Schemas;
using Stellaris.Shared.Storage;

namespace Stellaris.Shared.Business
{
    public class ChangeEventProcessingService(
        AzureDevOpsClientConfig azureDevOpsClientConfig,
        GeneralCosmosClient generalCosmosClient,
        FeedProcessorCosmosClient feedProcessorCosmosClient,
        CustomRoleAssignmentCosmosClient customRoleAssignmentCosmosClient,
        SecurityEnforcementService securityEnforcementService, 
        ILogger<ChangeEventProcessingService> logger,
        DevOpsClient devOpsClient) : BusinessServiceBase(azureDevOpsClientConfig, devOpsClient)
    {
        private ChangeFeedProcessor? _changeFeedProcessor;

        public async Task StartAsync()
        {
            await generalCosmosClient.InitializeDatabaseAsync();

            _changeFeedProcessor = await feedProcessorCosmosClient.StartListeningForEventsAsync(HandleChangesAsync);
        }

        public async Task StopAsync()
        {
            if (_changeFeedProcessor != null)
            {
                await feedProcessorCosmosClient.StopListeningAsync(_changeFeedProcessor);
            }
        }

        public async Task HandleChangesAsync(
            IReadOnlyCollection<ChangeEventEntity> changes, CancellationToken cancellationToken)
        {
            var projectAndFolderIdCollection = await CollectAllFolderIdAsync(customRoleAssignmentCosmosClient, changes);
            foreach(var tuple in projectAndFolderIdCollection)
            {
                await securityEnforcementService.EnforceBatchFromDaemonServiceAsync(tuple.ProjectId, tuple.FolderId);
            }
            logger.LogInformation("Finished handling changes.");
        }

        private async Task<List<ProjectIdAndFolderId>> CollectAllFolderIdAsync(
            CustomRoleAssignmentCosmosClient customRoleAssignmentCosmosClient,             
            IReadOnlyCollection<ChangeEventEntity> changes)
        {
            List<ProjectIdAndFolderId> projectAndFolderIdCollection = [];
            foreach (var changeEventEntity in changes)
            {
                logger.LogInformation($"Detected operation for item with id {changeEventEntity.Id}");

                if (!string.IsNullOrWhiteSpace(changeEventEntity.ProjectId))
                {
                    if (string.IsNullOrWhiteSpace(changeEventEntity.FolderId))
                    {
                        if (Enum.TryParse<ChangeEventType>(changeEventEntity.EventType, out var parsedEventType) &&
                            (parsedEventType == ChangeEventType.CustomRoleCreated 
                            || parsedEventType == ChangeEventType.CustomRoleUpdated
                            || parsedEventType == ChangeEventType.CustomRoleDeleted))
                        {
                            if (!string.IsNullOrEmpty(changeEventEntity.CustomRoleId))
                            {
                                var roleAssignments = await customRoleAssignmentCosmosClient.ListCustomRolesAssignmentsByCustomRoleIdAsync(
                                    changeEventEntity.ProjectId, changeEventEntity.CustomRoleId);
                                if (roleAssignments != null && roleAssignments.Count != 0)
                                {
                                    foreach (var roleAssignment in roleAssignments)
                                    {
                                        AddIfNotExists(projectAndFolderIdCollection, roleAssignment.ProjectId, roleAssignment.FolderId);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        AddIfNotExists(projectAndFolderIdCollection, changeEventEntity.ProjectId, changeEventEntity.FolderId);
                    }
                }
            }
            return projectAndFolderIdCollection;
        }

        private void AddIfNotExists(List<ProjectIdAndFolderId> projectAndFolderIdCollection, string projectId, string folderId)
        {
            var combinedItem = new ProjectIdAndFolderId(projectId, folderId);
            if (!projectAndFolderIdCollection.Exists(x => x.UniqueId == combinedItem.UniqueId))
            {
                projectAndFolderIdCollection.Add(combinedItem);
            }
        }

        private class ProjectIdAndFolderId
        {
            public ProjectIdAndFolderId(string projectId, string folderId)
            {
                ProjectId = projectId;
                FolderId = folderId;
            }

            public string ProjectId { get; set; } = string.Empty;
            public string FolderId { get; set; } = string.Empty;

            public string UniqueId => $"{ProjectId}_{FolderId}";
        }
    }
}
