

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Stellaris.Shared.Config;
using Stellaris.Shared.Schemas;
using Stellaris.Shared.Storage.Abstracts;

namespace Stellaris.Shared.Storage
{
    public class SecurityEnforcementCosmosClient(CosmosDbConfig cosmosDbConfig, ILogger<SecurityEnforcementCosmosClient> logger)
        : CosmosClientBase(cosmosDbConfig, logger)
    {
        protected override string ContainerId => ContainerConstants.CONTAINERID_SECURITY_ENFORCEMENT;

        public async Task<SecurityEnforcementLog> UpdateEnforcementStatusAsync(
            string projectId, string folderId, string resourceId, string resourceKind, EnforcementStatus status)
        {
            var entity = await CreateIfNotExistedAsync(projectId, folderId, resourceId, resourceKind);
            entity.EnforcementStatus = status;
            entity.LastEnforcedAt = DateTime.UtcNow;
            var updatedEntity = await Container.UpsertItemAsync(entity, new PartitionKey(entity.PartitionKey));
            return updatedEntity.Resource;
        }

        public async Task<SecurityEnforcementLog> CreateIfNotExistedAsync(
            string projectId, string folderId, string resourceId, string resourceKind)
        {
            var enforceLogs = await ListSecurityEnforcementLogAsync(projectId, folderId, resourceId, resourceKind);
            SecurityEnforcementLog? entity;
            if (enforceLogs.Count != 0)
            {
                entity = enforceLogs.First();
            }
            else
            {
                entity = new SecurityEnforcementLog
                {
                    ProjectId = projectId,
                    FolderId = folderId,
                    ResourceId = resourceId,
                    ResourceKind = resourceKind,
                    LastEnforcedAt = DateTime.UtcNow,
                    EnforcementStatus = EnforcementStatus.NOT_ENFORCED
                };
                await Container.CreateItemAsync(entity, new PartitionKey(entity.PartitionKey));
            }
            return entity;
        }

        private async Task<List<SecurityEnforcementLog>> ListSecurityEnforcementLogAsync(
            string projectId, string folderId, string resourceId, string resourceKind)
        {
            ArgumentNullException.ThrowIfNull(projectId);
            ArgumentNullException.ThrowIfNull(folderId);
            ArgumentNullException.ThrowIfNull(resourceId);
            ArgumentNullException.ThrowIfNull(resourceKind);

            var query = new QueryDefinition("SELECT * FROM c WHERE c.projectId = @projectId and c.resourceKind = @resourceKind")
                .WithParameter("@projectId", projectId)
                .WithParameter("@resourceKind", resourceKind);

            var enforceLogs = new List<SecurityEnforcementLog>();
            var iterator = Container.GetItemQueryIterator<SecurityEnforcementLog>(query,
                requestOptions: new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey(SecurityEnforcementLog.GetPartitionKey(folderId, resourceId))
                });
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                enforceLogs.AddRange(response);
            }
            return enforceLogs;
        }

    }
}
