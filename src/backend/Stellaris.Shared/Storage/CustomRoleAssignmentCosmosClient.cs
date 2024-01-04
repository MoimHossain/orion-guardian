

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Stellaris.Shared.Config;
using Stellaris.Shared.Schemas;
using Stellaris.Shared.Storage.Abstracts;
using System.Net;

namespace Stellaris.Shared.Storage
{
    public class CustomRoleAssignmentCosmosClient(CosmosDbConfig cosmosDbConfig, ILogger<CustomRoleCosmosClient> logger)
        : CosmosClientBase(cosmosDbConfig, logger)
    {
        public async Task<CustomRoleAssignmentEntity> CreateAsync(
            string projectId, string folderId, Identity identity, string customRoleId)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(projectId, nameof(projectId));            
            ArgumentNullException.ThrowIfNull(identity, nameof(identity));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(customRoleId, nameof(customRoleId));
            
            var assignmentEntity = new CustomRoleAssignmentEntity
            {
                ProjectId = projectId,
                FolderId = folderId,
                Identity = identity,
                CustomRoleId = customRoleId
            };
            CustomRoleAssignmentEntity updatedItem = await Container.UpsertItemAsync<CustomRoleAssignmentEntity>(
                item: assignmentEntity,
                partitionKey: new PartitionKey(assignmentEntity.ProjectId)
            );
            return updatedItem;
        }

        public async Task<CustomRoleAssignmentEntity?> DeleteAsync(string projectId, string roleAssignmentId)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(projectId, nameof(projectId));
            ArgumentNullException.ThrowIfNullOrEmpty(roleAssignmentId, nameof(roleAssignmentId));

            try
            {
                var itemToDelete = await Container.ReadItemAsync<CustomRoleAssignmentEntity>(roleAssignmentId, new PartitionKey(projectId));
                if(itemToDelete != null)
                {
                    var response = await Container.DeleteItemAsync<CustomRoleAssignmentEntity>(roleAssignmentId, new PartitionKey(projectId));
                    return itemToDelete;
                }
            }
            catch (CosmosException dbException) when (dbException.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.LogError($"Role assignment not found: {roleAssignmentId}");
            }
            return default;
        }

        public async Task<List<CustomRoleAssignmentEntity>> ListCustomRolesAssignmentsAsync(
            string projectId, string folderId)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.projectId = @projectId and c.folderId = @folderId ")
               .WithParameter("@projectId", projectId)
               .WithParameter("@folderId", folderId);

            var roleAssignments = new List<CustomRoleAssignmentEntity>();
            var iterator = Container.GetItemQueryIterator<CustomRoleAssignmentEntity>(query,
                               requestOptions: new QueryRequestOptions()
                               {
                                   PartitionKey = new PartitionKey(projectId)
                               });
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                roleAssignments.AddRange(response);
            }
            return roleAssignments;
        }

        public async Task<List<CustomRoleAssignmentEntity>> ListCustomRolesAssignmentsByCustomRoleIdAsync(
            string projectId, string customRoleId)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.projectId = @projectId and c.customRoleId = @customRoleId ")
               .WithParameter("@projectId", projectId)
               .WithParameter("@customRoleId", customRoleId);

            var roleAssignments = new List<CustomRoleAssignmentEntity>();
            var iterator = Container.GetItemQueryIterator<CustomRoleAssignmentEntity>(query,
                               requestOptions: new QueryRequestOptions()
                               {
                                   PartitionKey = new PartitionKey(projectId)
                               });
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                roleAssignments.AddRange(response);
            }
            return roleAssignments;
        }

        protected override string ContainerId => ContainerConstants.CONTAINERID_CUSTOMROLES_ASSIGNMENTS;
    }
}
