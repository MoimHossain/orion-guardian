

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Stellaris.Shared.Config;
using Stellaris.Shared.Schemas;
using Stellaris.Shared.Storage.Abstracts;
using System.ComponentModel;
using System.Net;
using User = Stellaris.Shared.Schemas.User;

namespace Stellaris.Shared.Storage
{
    public class FolderCosmosClient(CosmosDbConfig cosmosDbConfig, ILogger<FolderCosmosClient> logger) : CosmosClientBase(cosmosDbConfig, logger)
    {
        public async Task<bool> DeleteFolderAsync(string projectId, string id)
        {
            if (string.IsNullOrWhiteSpace(projectId))
                throw new ArgumentNullException(nameof(projectId));
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));

            var folderToBeDeleted = await FindFolderById(projectId, id);
            if(folderToBeDeleted != null)
            {
                var bucket = new List<(string, string)>
                {
                    (projectId, id)
                };
                await ListFoldersForDeletionAsync(projectId, id, bucket);

                //foreach (var (projectId, folderId) in bucket)
                //{
                //    var links = await linkCosmosClient.ListLinksByFolderIdAsync(projectId, folderId);
                //    if (links != null)
                //    {
                //        foreach (var link in links)
                //        {
                //            await linkCosmosClient.DeleteLinkAsync(projectId, link.Id);
                //        }
                //    }
                //    var securityDescriptors = await securityCosmosClient.ListSecurityDescriptorsByFolderIdAsync(projectId, folderId);
                //    if (securityDescriptors != null)
                //    {
                //        foreach (var sd in securityDescriptors)
                //        {
                //            await securityCosmosClient.DeleteSecurityDescriptorAsync(projectId, sd.Id);
                //        }
                //    }
                //}

                foreach (var (pId, folderId) in bucket)
                {
                    await Container.DeleteItemAsync<FolderEntity>(folderId, new PartitionKey(projectId));
                }

                // todo correct the hasChildren flag
            }
            return true;
        }

        public async Task<List<FolderEntity>> ListRootFoldersAsync(string projectId)
        {
            return await ListFoldersAsync(projectId, parentFolderId: projectId);
        }

        public async Task<List<FolderEntity>> ListChildFoldersAsync(string projectId, string parentFolderId)
        {
            return await ListFoldersAsync(projectId, parentFolderId: parentFolderId);
        }



        public async Task<FolderEntity> UpdateFolderAsync(
            string projectId, string folderId, string folderName, string kind,
            string description, string ciNumber, User lastModifiedBy)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(projectId, nameof(projectId));
            ArgumentNullException.ThrowIfNullOrEmpty(folderId, nameof(folderId));
            ArgumentNullException.ThrowIfNullOrEmpty(folderName, nameof(folderName));
            ArgumentNullException.ThrowIfNullOrEmpty(kind, nameof(kind));            
            ArgumentNullException.ThrowIfNull(lastModifiedBy, nameof(lastModifiedBy));

            
            var folder = await FindFolderById(projectId, folderId);

            if(folder == null)
                throw new Exception($"Folder with id {folderId} not found");

            folder.Name = folderName;
            folder.Kind = kind;
            folder.Description = description;
            folder.CINumber = ciNumber;
            folder.LastModified = DateTime.UtcNow;
            folder.LastModifiedBy = lastModifiedBy;

            //FixFolderPathsCore(topLevelFolders.Folders);

            FolderEntity folderEntity = await Container.UpsertItemAsync<FolderEntity>(
                item: folder,
                partitionKey: new PartitionKey(projectId)
            );
            return folderEntity;
        }

        public async Task<FolderEntity> CreateSubFolderAsync(
            string projectId, string parentFolderId, string folderName, string kind,
            string description, string ciNumber, User createdBy)
        {
            if (string.IsNullOrWhiteSpace(projectId))
                throw new ArgumentNullException(nameof(projectId));
            if (string.IsNullOrWhiteSpace(parentFolderId))
                throw new ArgumentNullException(nameof(parentFolderId));
            if (string.IsNullOrWhiteSpace(folderName))
                throw new ArgumentNullException(nameof(folderName));
            if (string.IsNullOrWhiteSpace(kind))
                throw new ArgumentNullException(nameof(kind));

            
            var parentFolder = await FindFolderById(projectId, parentFolderId);
            if (parentFolder == null)
                throw new Exception($"Parent folder with id {parentFolderId} not found");

            var folderEntity = new FolderEntity()
            {
                ProjectId = projectId,
                Id = Guid.NewGuid().ToString(),
                ParentFolderId = parentFolder.Id,
                Name = folderName,
                Kind = kind,
                Path = $"{parentFolder.Path}/{folderName}",
                Description = description,
                CINumber = ciNumber,
                CreatedOn = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                CreatedBy = createdBy,
                LastModifiedBy = createdBy
            };

            FolderEntity childFolderEntity = await Container.UpsertItemAsync<FolderEntity>(
                item: folderEntity,
                partitionKey: new PartitionKey(projectId)
            );
            
            parentFolder.HasChildren = true;
            await Container.UpsertItemAsync<FolderEntity>(
                item: parentFolder,
                partitionKey: new PartitionKey(projectId)
            );
            return childFolderEntity;
        }

        public async Task<FolderEntity> CreateTopLevelFolderAsync(
            string projectId, string folderName, string kind,
            string description, string ciNumber, 
            User createdBy)
        {
            if (string.IsNullOrWhiteSpace(projectId))
                throw new ArgumentNullException(nameof(projectId));
            if (string.IsNullOrWhiteSpace(folderName))
                throw new ArgumentNullException(nameof(folderName));
            if (string.IsNullOrWhiteSpace(kind))
                throw new ArgumentNullException(nameof(kind));

            var entity = new FolderEntity()
            {
                ProjectId = projectId,
                Id = Guid.NewGuid().ToString(),
                Name = folderName,
                Path = folderName,
                Kind = kind,
                ParentFolderId = projectId, // Let's use Project ID it self as parent folder id for top level folders
                Description = description,
                CINumber = ciNumber,
                CreatedOn = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                CreatedBy = createdBy,
                LastModifiedBy = createdBy,
                HasChildren = false
            };

            // TODO: Check if this is needed
            //var folderCollection = await GetTopLevelFoldersAsync(projectId);
            //folderCollection.Folders.Add();
            //FixFolderPathsCore(folderCollection.Folders);

            FolderEntity topLevelFolderEntity = await Container.UpsertItemAsync<FolderEntity>(
                item: entity,
                partitionKey: new PartitionKey(projectId)
            );
            return topLevelFolderEntity;
        }

        private async Task<FolderEntity?> FindFolderById(string projectId, string folderId)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(projectId, nameof(projectId));
            ArgumentNullException.ThrowIfNullOrEmpty(folderId, nameof(folderId));

            try
            {
                var folderEntity = await Container.ReadItemAsync<FolderEntity>(
                    id: folderId,
                    partitionKey: new PartitionKey(projectId)
                );
                return folderEntity.Resource;
            }
            catch (CosmosException dbException) when (dbException.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.LogError($"Project folder not found: {projectId} & {folderId}");
            }
            return default;
        }

        private async Task ListFoldersForDeletionAsync(string projectId, string folderId, List<(string, string)> bucket)
        {
            var children = await ListChildFoldersAsync(projectId, folderId);
            if (children != null)
            {
                foreach (var child in children)
                {
                    bucket.Add((projectId, child.Id));
                    await ListFoldersForDeletionAsync(projectId, child.Id, bucket);
                }
            }
        }

        private async Task<List<FolderEntity>> ListFoldersAsync(string projectId, string parentFolderId)
        {
            if (string.IsNullOrWhiteSpace(projectId))
                throw new ArgumentNullException(nameof(projectId));

            var query = new QueryDefinition("SELECT * FROM c WHERE c.projectId = @projectId and c.parentFolderId = @parentFolderId")
                .WithParameter("@projectId", projectId)
                .WithParameter("@parentFolderId", parentFolderId);

            var rootFolders = new List<FolderEntity>();
            var iterator = Container.GetItemQueryIterator<FolderEntity>(query,
                               requestOptions: new QueryRequestOptions()
                               {
                                   PartitionKey = new PartitionKey(projectId)
                               });
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                rootFolders.AddRange(response);
            }
            return rootFolders;
        }

        protected override string ContainerId => ContainerConstants.CONTAINERID_FOLDERS;
    }
}
