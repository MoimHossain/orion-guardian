
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Stellaris.Shared.Config;
using Stellaris.Shared.Schemas;
using Stellaris.Shared.Storage.Abstracts;

namespace Stellaris.Shared.Storage
{

    public class AuditCosmosClient(CosmosDbConfig cosmosDbConfig, ILogger<AuditCosmosClient> logger) 
        : CosmosClientBase(cosmosDbConfig, logger)
    {
        protected override string ContainerId => ContainerConstants.CONTAINERID_AUDIT;

        

        public async Task<AuditEntity> CreateAuditEventAsync(
            string projectId, AuditEntity auditEntity)
        {
            if (string.IsNullOrWhiteSpace(projectId))
                throw new ArgumentNullException(nameof(projectId));


            AuditEntity updatedItem = await Container.UpsertItemAsync<AuditEntity>(
                item: auditEntity,
                partitionKey: new PartitionKey(projectId)
            );
            return updatedItem;
        }
    }
}
