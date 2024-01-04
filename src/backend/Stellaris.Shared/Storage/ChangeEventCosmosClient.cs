

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Stellaris.Shared.Config;
using Stellaris.Shared.Schemas;
using Stellaris.Shared.Storage.Abstracts;

namespace Stellaris.Shared.Storage
{
    public class ChangeEventCosmosClient(CosmosDbConfig cosmosDbConfig, ILogger<ChangeEventCosmosClient> logger) 
        : CosmosClientBase(cosmosDbConfig, logger)
    {
        protected override string ContainerId => ContainerConstants.CONTAINERID_CHANGE_EVENTS;
        
        public async Task<ChangeEventEntity> RaiseEventAsync(ChangeEventEntity changeEventEntity)
        {
            ArgumentNullException.ThrowIfNull(changeEventEntity, nameof(changeEventEntity));
            ArgumentException.ThrowIfNullOrEmpty(changeEventEntity.ProjectId, nameof(changeEventEntity.ProjectId));

            var updatedItem = await Container.UpsertItemAsync(
                item: changeEventEntity,
                partitionKey: new PartitionKey(changeEventEntity.ProjectId)
            );
            return updatedItem;
        }
    }
}
