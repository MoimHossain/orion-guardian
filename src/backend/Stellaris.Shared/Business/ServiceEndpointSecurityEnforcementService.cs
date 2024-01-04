


using Microsoft.Extensions.Logging;
using Stellaris.Shared.AzureDevOps;
using Stellaris.Shared.AzureDevOps.DTOs;
using Stellaris.Shared.AzureDevOps.Support;
using Stellaris.Shared.Business.Abstractions;
using Stellaris.Shared.Config;
using Stellaris.Shared.Schemas;
using Stellaris.Shared.Storage;
using static Stellaris.Shared.AzureDevOps.SecurityNamespaces.PermissionEnums;
using Identity = Stellaris.Shared.Schemas.Identity;

namespace Stellaris.Shared.Business
{
    public class ServiceEndpointSecurityEnforcementService(
        AzureDevOpsClientConfig azureDevOpsClientConfig,
        BuiltInAccountService builtInAccountService,
        LinkCosmosClient linkCosmosClient,
        SecurityEnforcementCosmosClient securityEnforcementCosmosClient,
        ILogger<ServiceEndpointSecurityEnforcementService> logger,
        DevOpsClient devOpsClient) : ResourceSecurityEnforcementBase(azureDevOpsClientConfig, builtInAccountService,
            linkCosmosClient, securityEnforcementCosmosClient, devOpsClient)
    {
        private const string BUILTIN_SERVICE_ENDPOINT_ADMINISTRATOR_ROLE_NAME = "Administrator";
        private const string BUILTIN_SERVICE_ENDPOINT_READER_ROLE_NAME = "Reader";

        protected async override Task EnforceCoreAsync(
            string projectId, string folderId, string resourceId, 
            IEnumerable<CustomRoleDefinitionEntity> customRoles, 
            IEnumerable<CustomRoleAssignmentEntity> roleAssignments, 
            IEnumerable<Identity> folderAdministrators, 
            SecurityDescriptorTranslator translator)
        {
            // disable inheritance
            await DevOpsClient.ChangeServiceEndpointInheritanceAsync(AzureDevOpsClientConfig.orgName,
                projectId, resourceId, inheritPermissions: false);

            // remove all existing permissions
            var existingRoleAssignments = await DevOpsClient
                .ListServiceEndpointRoleAssignmentsAsync(AzureDevOpsClientConfig.orgName, projectId, resourceId);

            if (existingRoleAssignments != null && existingRoleAssignments.Value != null && existingRoleAssignments.Value.Any())
            {
                await DevOpsClient.RemoveServiceEndpointRolesAsync(AzureDevOpsClientConfig.orgName,
                    projectId, resourceId, existingRoleAssignments.Value.Select(era => era.Identity.Id));
            }

            List<AzDoRoleAssignment> azDoServiceEndpointRoleAssignments = [];

            var pcaAdministrators = await AccountService.GetProjectCollectionAdministratorsAsync(projectId);
            if (pcaAdministrators != null && !string.IsNullOrWhiteSpace(pcaAdministrators.LocalId))
            {
                azDoServiceEndpointRoleAssignments.Add(new AzDoRoleAssignment(
                    UserId: Guid.Parse(pcaAdministrators.LocalId),
                    RoleName: BUILTIN_SERVICE_ENDPOINT_ADMINISTRATOR_ROLE_NAME));
            }

            var projectAdministrators = await AccountService.GetProjectAdministratorsAsync(projectId);
            if (projectAdministrators != null && !string.IsNullOrWhiteSpace(projectAdministrators.LocalId))
            {
                azDoServiceEndpointRoleAssignments.Add(new AzDoRoleAssignment(
                    UserId: Guid.Parse(projectAdministrators.LocalId),
                    RoleName: BUILTIN_SERVICE_ENDPOINT_READER_ROLE_NAME));
            }

            // Folder admins are automatically granted Reader role
            var folderAdminLocalIds = folderAdministrators
                .Where(fa => !IsSubjectAlreadyConfigured(fa.SubjectDescriptor, roleAssignments))
                .Select(fa => fa.LocalId);
            if (folderAdminLocalIds != null && folderAdminLocalIds.Any())
            {
                foreach (var folderAdminLocalId in folderAdminLocalIds)
                {
                    var alreadyExists = azDoServiceEndpointRoleAssignments.Exists(a => a.UserId == Guid.Parse(folderAdminLocalId));
                    if (!alreadyExists)
                    {
                        azDoServiceEndpointRoleAssignments.Add(new AzDoRoleAssignment(
                            UserId: Guid.Parse(folderAdminLocalId),
                            RoleName: BUILTIN_SERVICE_ENDPOINT_READER_ROLE_NAME));
                    }
                }
            }

            foreach (var roleAssignment in roleAssignments)
            {
                if (roleAssignment.Identity != null)
                {
                    var alreadyExists = azDoServiceEndpointRoleAssignments
                        .Exists(a => a.UserId == Guid.Parse(roleAssignment.Identity.LocalId));
                    if (!alreadyExists)
                    {
                        var customRole = customRoles
                            .FirstOrDefault(ca => ca.Id.Equals(roleAssignment.CustomRoleId, StringComparison.OrdinalIgnoreCase));
                        if (customRole != null && customRole.ServiceEndpointRole != null 
                            && !string.IsNullOrWhiteSpace(customRole.ServiceEndpointRole.Name))
                        {
                            azDoServiceEndpointRoleAssignments.Add(new AzDoRoleAssignment(
                                UserId: Guid.Parse(roleAssignment.Identity.LocalId),
                                RoleName: customRole.ServiceEndpointRole.Name));
                        }
                    }
                }
            }

            await DevOpsClient.AddServiceEndpointRolesAsync(
                AzureDevOpsClientConfig.orgName, projectId, resourceId, azDoServiceEndpointRoleAssignments);

            logger.LogInformation($"Enforce security for {GetResourceKindAsString()}:{resourceId} completed.");
        }

        protected override ResourceKind GetResourceKind() => ResourceKind.Endpoint;
    }
}
