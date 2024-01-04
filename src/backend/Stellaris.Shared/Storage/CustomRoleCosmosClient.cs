

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Stellaris.Shared.Config;
using Stellaris.Shared.Schemas;
using Stellaris.Shared.Storage.Abstracts;
using System.Net;

namespace Stellaris.Shared.Storage
{
    public class CustomRoleCosmosClient(CosmosDbConfig cosmosDbConfig, ILogger<CustomRoleCosmosClient> logger) 
        : CosmosClientBase(cosmosDbConfig, logger)
    {   
        private async Task<List<CustomRoleDefinitionEntity>> FindCustomRoleByName(string projectId, string name)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.projectId = @projectId and c.name = @name")
               .WithParameter("@projectId", projectId)
               .WithParameter("@name", name);

            var customRoleDefinitions = new List<CustomRoleDefinitionEntity>();
            var iterator = Container.GetItemQueryIterator<CustomRoleDefinitionEntity>(query,
                               requestOptions: new QueryRequestOptions()
                               {
                                   PartitionKey = new PartitionKey(projectId)
                               });
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                customRoleDefinitions.AddRange(response);
            }
            return customRoleDefinitions;
        }

        private async Task<CustomRoleDefinitionEntity?> FindById(string projectId, string roleId)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(projectId, nameof(projectId));
            ArgumentNullException.ThrowIfNullOrEmpty(roleId, nameof(roleId));

            try
            {
                var roleEntity = await Container.ReadItemAsync<CustomRoleDefinitionEntity>(
                    id: roleId,
                    partitionKey: new PartitionKey(projectId)
                );
                return roleEntity.Resource;
            }
            catch (CosmosException dbException) when (dbException.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.LogError($"Project folder not found: {projectId} & {roleId}");
            }
            return default;
        }

        public async Task<List<CustomRoleDefinitionEntity>> ListCustomRolesAsync(string projectId)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.projectId = @projectId ")
               .WithParameter("@projectId", projectId);

            var customRoleDefinitions = new List<CustomRoleDefinitionEntity>();
            var iterator = Container.GetItemQueryIterator<CustomRoleDefinitionEntity>(query,
                               requestOptions: new QueryRequestOptions()
                               {
                                   PartitionKey = new PartitionKey(projectId)
                               });
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                customRoleDefinitions.AddRange(response);
            }
            return customRoleDefinitions;
        }

        public async Task<bool> DeleteAsync(string projectId, string roleId)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(projectId, nameof(projectId));
            ArgumentNullException.ThrowIfNullOrEmpty(roleId, nameof(roleId));

            try
            {
                await Container.DeleteItemAsync<CustomRoleDefinitionEntity>(roleId, new PartitionKey(projectId));
                return true;
            }
            catch (CosmosException dbException) when (dbException.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.LogError($"Role not found: {roleId}");
            }
            return false;
        }

        public async Task<CustomRoleDefinitionEntity> ModifyAsync(
            string projectId, string roleId, string name,
            List<PermissionGrant> repositoryPermissionGrants,
            List<PermissionGrant> buildPermissionGrants,
            List<PermissionGrant> releasePermissionGrants,
            RoleDefinition serviceEndpointRole,
            RoleDefinition environmentRole,
            RoleDefinition libraryRole)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(projectId, nameof(projectId));
            ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
            ArgumentNullException.ThrowIfNull(repositoryPermissionGrants, nameof(repositoryPermissionGrants));
            ArgumentNullException.ThrowIfNull(buildPermissionGrants, nameof(buildPermissionGrants));
            ArgumentNullException.ThrowIfNull(releasePermissionGrants, nameof(releasePermissionGrants));
            ArgumentNullException.ThrowIfNull(serviceEndpointRole, nameof(serviceEndpointRole));
            ArgumentNullException.ThrowIfNull(environmentRole, nameof(environmentRole));
            ArgumentNullException.ThrowIfNull(libraryRole, nameof(libraryRole));

            var existingRole = await FindById(projectId, roleId);
            if (existingRole == null)
            {
                throw new ArgumentException($"Custom role with id {roleId} does not exist");
            }

            existingRole.Name = name;
            existingRole.RepositoryPermissionGrants = repositoryPermissionGrants;
            existingRole.BuildPermissionGrants = buildPermissionGrants;
            existingRole.ReleasePermissionGrants = releasePermissionGrants;
            existingRole.ServiceEndpointRole = serviceEndpointRole;
            existingRole.EnvironmentRole = environmentRole;
            existingRole.LibraryRole = libraryRole;

            var updatedItem = await Container
                .UpsertItemAsync<CustomRoleDefinitionEntity>(item: existingRole, partitionKey: new PartitionKey(projectId));
            return updatedItem;
        }

        public async Task<CustomRoleDefinitionEntity> CreateAsync(
            string projectId, string name,
            List<PermissionGrant> repositoryPermissionGrants,
            List<PermissionGrant> buildPermissionGrants,
            List<PermissionGrant> releasePermissionGrants,
            RoleDefinition serviceEndpointRole,
            RoleDefinition environmentRole,
            RoleDefinition libraryRole)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(projectId, nameof(projectId));
            ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
            ArgumentNullException.ThrowIfNull(repositoryPermissionGrants, nameof(repositoryPermissionGrants));
            ArgumentNullException.ThrowIfNull(buildPermissionGrants, nameof(buildPermissionGrants));
            ArgumentNullException.ThrowIfNull(releasePermissionGrants, nameof(releasePermissionGrants));
            ArgumentNullException.ThrowIfNull(serviceEndpointRole, nameof(serviceEndpointRole));
            ArgumentNullException.ThrowIfNull(environmentRole, nameof(environmentRole));
            ArgumentNullException.ThrowIfNull(libraryRole, nameof(libraryRole));

            var existingRoles = await FindCustomRoleByName(projectId, name);
            if(existingRoles.Any())
            {
                throw new ArgumentException($"Custom role with name {name} already exists for project {projectId}");
            }

            var roleEntity = new CustomRoleDefinitionEntity
            {
                ProjectId = projectId,
                Name = name,
                RepositoryPermissionGrants = repositoryPermissionGrants,
                BuildPermissionGrants = buildPermissionGrants,
                ReleasePermissionGrants = releasePermissionGrants,
                ServiceEndpointRole = serviceEndpointRole,
                EnvironmentRole = environmentRole,
                LibraryRole = libraryRole
            };
            CustomRoleDefinitionEntity updatedItem = await Container.UpsertItemAsync<CustomRoleDefinitionEntity>(
                item: roleEntity,
                partitionKey: new PartitionKey(projectId)
            );
            return updatedItem;
        }

        protected override string ContainerId => ContainerConstants.CONTAINERID_CUSTOMROLES;
    }
}
