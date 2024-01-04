
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Stellaris.Shared.Config;
using Stellaris.Shared.Schemas;
using Stellaris.Shared.Storage.Abstracts;
using System.Net;

namespace Stellaris.Shared.Storage
{
    public class SecurityCosmosClient(CosmosDbConfig cosmosDbConfig, ILogger<SecurityCosmosClient> logger) 
        : CosmosClientBase(cosmosDbConfig, logger)
    {
        protected override string ContainerId => ContainerConstants.CONTAINERID_SECURITY;

        public async Task<bool> DeleteSecurityDescriptorAsync(string projectId, string descriptorId)
        {
            if (string.IsNullOrWhiteSpace(projectId))
                throw new ArgumentNullException(nameof(projectId));
            if (string.IsNullOrWhiteSpace(descriptorId))
                throw new ArgumentNullException(nameof(descriptorId));
            try
            {
                await Container.DeleteItemAsync<SecurityDescriptorEntity>(descriptorId, new PartitionKey(projectId));
                return true;
            }
            catch (CosmosException dbException) when (dbException.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.LogError($"Security Descriptor not found: {descriptorId}");
            }
            return false;
        }

        public async Task<SecurityDescriptorEntity> CreateOrUpdatePermissionDescriptorAsync(
            string projectId, string folderId, string kind,
            List<PermissionDescriptor> permissionDescriptors)
        {
            if (string.IsNullOrWhiteSpace(projectId))
                throw new ArgumentNullException(nameof(projectId));
            if (string.IsNullOrWhiteSpace(folderId))
                throw new ArgumentNullException(nameof(folderId));
            if (string.IsNullOrWhiteSpace(kind))
                throw new ArgumentNullException(nameof(kind));
            if(permissionDescriptors == null)
                throw new ArgumentNullException(nameof(permissionDescriptors));

            foreach(var pd in permissionDescriptors)
            {
                if (pd.IsValid() == false)
                    throw new ArgumentException($"Invalid permission descriptor: {pd}");
            }

            var entity = await FindEntityOrCreateDefaultAsync(projectId, folderId, kind);
            if (permissionDescriptors != null)
            {
                entity.PermissionDescriptors.Clear();
                entity.PermissionDescriptors.AddRange(permissionDescriptors);
            }
            return await CreateOrUpdateDescriptorCoreAsync(projectId, entity);
        }

        public async Task<SecurityDescriptorEntity> CreateOrUpdateRoleDescriptorAsync(
            string projectId, string folderId, string kind,            
            List<RoleDescriptor> roleDescriptors)
        {
            if (string.IsNullOrWhiteSpace(projectId))
                throw new ArgumentNullException(nameof(projectId));
            if (string.IsNullOrWhiteSpace(folderId))
                throw new ArgumentNullException(nameof(folderId));
            if (string.IsNullOrWhiteSpace(kind))
                throw new ArgumentNullException(nameof(kind));
            if(roleDescriptors == null)
                throw new ArgumentNullException(nameof(roleDescriptors));

            foreach(var rd in roleDescriptors)
            {
                if (rd.IsValid() == false)
                    throw new ArgumentException($"Invalid role descriptor: {rd}");
            }

            var entity = await FindEntityOrCreateDefaultAsync(projectId, folderId, kind);

            if (roleDescriptors != null)
            {
                entity.RoleDescriptors.Clear();
                entity.RoleDescriptors.AddRange(roleDescriptors);
            }
            return await CreateOrUpdateDescriptorCoreAsync(projectId, entity);
        }

        public async Task<List<SecurityDescriptorEntity>> ListSecurityDescriptorsAsync(string projectId, string kind)
        {
            if (string.IsNullOrWhiteSpace(projectId))
                throw new ArgumentNullException(nameof(projectId));
            if (string.IsNullOrWhiteSpace(kind))
                throw new ArgumentNullException(nameof(kind));

            var query = new QueryDefinition("SELECT * FROM c WHERE c.projectId = @projectId and c.kind = @kind")
                .WithParameter("@projectId", projectId)
                .WithParameter("@kind", kind);

            var securityDescriptors = new List<SecurityDescriptorEntity>();
            var iterator = Container.GetItemQueryIterator<SecurityDescriptorEntity>(query,
                               requestOptions: new QueryRequestOptions()
                               {
                                   PartitionKey = new PartitionKey(projectId)
                               });
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                securityDescriptors.AddRange(response);
            }
            return securityDescriptors;
        }

        public async Task<List<SecurityDescriptorEntity>> ListSecurityDescriptorsAsync(string projectId, string folderId, string kind)
        {
            if (string.IsNullOrWhiteSpace(projectId))
                throw new ArgumentNullException(nameof(projectId));
            if (string.IsNullOrWhiteSpace(folderId))
                throw new ArgumentNullException(nameof(folderId));
            if (string.IsNullOrWhiteSpace(kind))
                throw new ArgumentNullException(nameof(kind));


            var query = new QueryDefinition("SELECT * FROM c WHERE c.projectId = @projectId and c.folderId = @folderId and c.kind = @kind")
                .WithParameter("@projectId", projectId)
                .WithParameter("@folderId", folderId)
                .WithParameter("@kind", kind);

            var securityDescriptors = new List<SecurityDescriptorEntity>();
            var iterator = Container.GetItemQueryIterator<SecurityDescriptorEntity>(query,
                               requestOptions: new QueryRequestOptions()
                               {
                                   PartitionKey = new PartitionKey(projectId)
                               });
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                securityDescriptors.AddRange(response);
            }
            return securityDescriptors;
        }

        public async Task<List<SecurityDescriptorEntity>> ListSecurityDescriptorsByFolderIdAsync(string projectId, string folderId)
        {
            if (string.IsNullOrWhiteSpace(projectId))
                throw new ArgumentNullException(nameof(projectId));
            if (string.IsNullOrWhiteSpace(folderId))
                throw new ArgumentNullException(nameof(folderId));


            var query = new QueryDefinition("SELECT * FROM c WHERE c.projectId = @projectId and c.folderId = @folderId")
                .WithParameter("@projectId", projectId)
                .WithParameter("@folderId", folderId);

            var securityDescriptors = new List<SecurityDescriptorEntity>();
            var iterator = Container.GetItemQueryIterator<SecurityDescriptorEntity>(query,
                               requestOptions: new QueryRequestOptions()
                               {
                                   PartitionKey = new PartitionKey(projectId)
                               });
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                securityDescriptors.AddRange(response);
            }
            return securityDescriptors;
        }


        private async Task<SecurityDescriptorEntity> CreateOrUpdateDescriptorCoreAsync(
            string projectId, SecurityDescriptorEntity entity)
        {
            SecurityDescriptorEntity updatedEntity = await Container.UpsertItemAsync<SecurityDescriptorEntity>(
                item: entity,
                partitionKey: new PartitionKey(projectId)
            );
            return updatedEntity;
        }

        private async Task<SecurityDescriptorEntity> FindEntityOrCreateDefaultAsync(string projectId, string folderId, string kind)
        {
            var securityDescriptors = await ListSecurityDescriptorsAsync(projectId, folderId, kind);
            SecurityDescriptorEntity? entity;
            if (securityDescriptors.Count != 0)
            {
                entity = securityDescriptors.First();
            }
            else
            {
                entity = new SecurityDescriptorEntity
                {
                    ProjectId = projectId,
                    FolderId = folderId,
                    Kind = kind
                };
                await Container.CreateItemAsync(entity, new PartitionKey(projectId));
            }
            return entity;
        }
    }
}
