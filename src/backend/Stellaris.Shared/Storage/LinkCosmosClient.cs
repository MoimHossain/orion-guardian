

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Stellaris.Shared.Config;
using Stellaris.Shared.Schemas;
using Stellaris.Shared.Storage.Abstracts;
using System.Net;
using User = Stellaris.Shared.Schemas.User;

namespace Stellaris.Shared.Storage
{
    public class LinkCosmosClient(CosmosDbConfig cosmosDbConfig, ILogger<LinkCosmosClient> logger) : CosmosClientBase(cosmosDbConfig, logger)
    {
        protected override string ContainerId => ContainerConstants.CONTAINERID_LINKS;

        public async Task<bool> DeleteLinkAsync(string projectId, string linkId)
        {
            if (string.IsNullOrWhiteSpace(projectId))
                throw new ArgumentNullException(nameof(projectId));
            if (string.IsNullOrWhiteSpace(linkId))
                throw new ArgumentNullException(nameof(linkId));
            try
            {
                await Container.DeleteItemAsync<LinkEntity>(linkId, new PartitionKey(projectId));
                return true;
            }
            catch (CosmosException dbException) when (dbException.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.LogError($"Link not found: {linkId}");
            }
            return false;
        }

        public async Task<List<LinkEntity>> ListLinksByFolderIdAsync(string projectId, string folderId)
        {
            if (string.IsNullOrWhiteSpace(projectId))
                throw new ArgumentNullException(nameof(projectId));
            if (string.IsNullOrWhiteSpace(folderId))
                throw new ArgumentNullException(nameof(folderId));

            var query = new QueryDefinition("SELECT * FROM c WHERE c.projectId = @projectId and c.folderId = @folderId")
                .WithParameter("@projectId", projectId)
                .WithParameter("@folderId", folderId);

            var links = new List<LinkEntity>();
            var iterator = Container.GetItemQueryIterator<LinkEntity>(query,
                               requestOptions: new QueryRequestOptions()
                               {
                                   PartitionKey = new PartitionKey(projectId)
                               });
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                links.AddRange(response);
            }
            return links;
        }

        public async Task<LinkEntity> CreateLinkAsync(
            string projectId, string folderId, string resourceId, string kind, string resourceName, User by)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(projectId, nameof(projectId));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(folderId, nameof(folderId));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(resourceId, nameof(resourceId));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(kind, nameof(kind));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(resourceName, nameof(resourceName));

            var existingLinks = await ListLinksByResourceIdAsync(projectId, kind, folderId, resourceId);
            if (existingLinks.Any())
            {
                return existingLinks.First();
            }

            var linkEntity = new LinkEntity
            {
                ProjectId = projectId,
                FolderId = folderId,
                ResourceId = resourceId,
                ResourceName = resourceName,                
                Kind = kind,
                CreatedOn = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                CreatedBy = by,
                LastModifiedBy = by
            };
            LinkEntity updatedItem = await Container.UpsertItemAsync<LinkEntity>(
                item: linkEntity,
                partitionKey: new PartitionKey(projectId)
            );
            return updatedItem;
        }



        public async Task<List<LinkEntity>> ListLinksByFolderIdAsync(string projectId, string kind, string folderId)
        {
            if (string.IsNullOrWhiteSpace(projectId))
                throw new ArgumentNullException(nameof(projectId));
            if (string.IsNullOrWhiteSpace(kind))
                throw new ArgumentNullException(nameof(kind));
            if (string.IsNullOrWhiteSpace(folderId))
                throw new ArgumentNullException(nameof(folderId));

            var query = new QueryDefinition("SELECT * FROM c WHERE c.projectId = @projectId and c.kind = @kind and c.folderId = @folderId")
                .WithParameter("@projectId", projectId)
                .WithParameter("@kind", kind)
                .WithParameter("@folderId", folderId);

            var links = new List<LinkEntity>();
            var iterator = Container.GetItemQueryIterator<LinkEntity>(query,
                               requestOptions: new QueryRequestOptions()
                               {
                                   PartitionKey = new PartitionKey(projectId)
                               });
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                links.AddRange(response);
            }
            return links;
        }

        public async Task<List<LinkEntity>> ListLinksByResourceIdAsync(string projectId, string kind, string folderId, string resourceId)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(projectId, nameof(projectId));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(kind, nameof(kind));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(folderId, nameof(folderId));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(resourceId, nameof(resourceId));

            var query = new QueryDefinition("SELECT * FROM c WHERE c.projectId = @projectId and c.kind = @kind and c.folderId = @folderId and c.resourceId = @resourceId")
                .WithParameter("@projectId", projectId)
                .WithParameter("@kind", kind)
                .WithParameter("@folderId", folderId)
                .WithParameter("@resourceId", resourceId);

            var links = new List<LinkEntity>();
            var iterator = Container.GetItemQueryIterator<LinkEntity>(query,
                                              requestOptions: new QueryRequestOptions()
                                              {
                                   PartitionKey = new PartitionKey(projectId)
                               });
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                links.AddRange(response);
            }
            return links;
        }

        public async Task UpdateSecurityEnforcementStampAsync(string projectId, string id, EnforcementStatus enforcementStatus)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id, nameof(id));

            var linkEntityWrapper = await Container.ReadItemAsync<LinkEntity>(id, new PartitionKey(projectId));
            if (linkEntityWrapper != null && linkEntityWrapper.Resource != null)
            {
                var linkEntity = linkEntityWrapper.Resource;
                linkEntity.EnforcementStatus = new LinkedResourceSecurityEnforcementStatus
                {
                    LastEnforcedAt = DateTime.UtcNow,
                    EnforcementStatus = enforcementStatus.ToString()
                };
                
                await Container.ReplaceItemAsync(linkEntity, id, new PartitionKey(projectId));
            }
        }
    }
}
