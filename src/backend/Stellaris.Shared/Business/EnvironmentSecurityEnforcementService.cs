

using Microsoft.Extensions.Logging;
using Stellaris.Shared.AzureDevOps;
using Stellaris.Shared.AzureDevOps.DTOs;
using Stellaris.Shared.AzureDevOps.Support;
using Stellaris.Shared.Business.Abstractions;
using Stellaris.Shared.Config;
using Stellaris.Shared.Schemas;
using Stellaris.Shared.Storage;

namespace Stellaris.Shared.Business
{
    public class EnvironmentSecurityEnforcementService(
        AzureDevOpsClientConfig azureDevOpsClientConfig,
        BuiltInAccountService builtInAccountService,
        LinkCosmosClient linkCosmosClient,
        SecurityEnforcementCosmosClient securityEnforcementCosmosClient,
        ILogger<EnvironmentSecurityEnforcementService> logger,
        DevOpsClient devOpsClient) : ResourceSecurityEnforcementBase(azureDevOpsClientConfig, builtInAccountService,
            linkCosmosClient, securityEnforcementCosmosClient, devOpsClient)
    {
        private const string BUILTIN_ENVIRONMENT_ADMINISTRATOR_ROLE_NAME = "Administrator";
        private const string BUILTIN_ENVIRONMENT_READER_ROLE_NAME = "Reader";

        protected async override Task EnforceCoreAsync(
            string projectId, string folderId, string resourceId,
            IEnumerable<CustomRoleDefinitionEntity> customRoles,
            IEnumerable<CustomRoleAssignmentEntity> roleAssignments,
            IEnumerable<Identity> folderAdministrators,
            SecurityDescriptorTranslator translator)
        {
            if (Int32.TryParse(resourceId, out var environmentId))
            {
                // disable inheritance
                await DevOpsClient.ChangeEnvironmentInheritanceAsync(AzureDevOpsClientConfig.orgName,
                    projectId, environmentId, inheritPermissions: false);

                // remove all existing permissions
                var existingRoleAssignments = await DevOpsClient
                    .ListEnvironmentRoleAssignmentsAsync(AzureDevOpsClientConfig.orgName, projectId, environmentId);

                if (existingRoleAssignments != null && existingRoleAssignments.Value != null && existingRoleAssignments.Value.Any())
                {
                    await DevOpsClient.RemoveEnvironmentRolesAsync(AzureDevOpsClientConfig.orgName,
                        projectId, environmentId, existingRoleAssignments.Value.Select(era => era.Identity.Id));
                }

                List<AzDoRoleAssignment> azDoEnvRoleAssignments = [];

                var pcaAdministrators = await AccountService.GetProjectCollectionAdministratorsAsync(projectId);
                if (pcaAdministrators != null && !string.IsNullOrWhiteSpace(pcaAdministrators.LocalId))
                {
                    azDoEnvRoleAssignments.Add(new AzDoRoleAssignment(
                        UserId: Guid.Parse(pcaAdministrators.LocalId),
                        RoleName: BUILTIN_ENVIRONMENT_ADMINISTRATOR_ROLE_NAME));
                }

                var projectAdministrators = await AccountService.GetProjectAdministratorsAsync(projectId);
                if (projectAdministrators != null && !string.IsNullOrWhiteSpace(projectAdministrators.LocalId))
                {
                    azDoEnvRoleAssignments.Add(new AzDoRoleAssignment(
                        UserId: Guid.Parse(projectAdministrators.LocalId),
                        RoleName: BUILTIN_ENVIRONMENT_ADMINISTRATOR_ROLE_NAME));
                }

                // Folder admins are automatically granted Reader role
                var folderAdminLocalIds = folderAdministrators
                    .Where(fa => !IsSubjectAlreadyConfigured(fa.SubjectDescriptor, roleAssignments))
                    .Select(fa => fa.LocalId);
                if (folderAdminLocalIds != null && folderAdminLocalIds.Any())
                {
                    foreach (var folderAdminLocalId in folderAdminLocalIds)
                    {
                        var alreadyExists = azDoEnvRoleAssignments.Exists(a => a.UserId == Guid.Parse(folderAdminLocalId));
                        if (!alreadyExists)
                        {
                            azDoEnvRoleAssignments.Add(new AzDoRoleAssignment(
                                UserId: Guid.Parse(folderAdminLocalId),
                                RoleName: BUILTIN_ENVIRONMENT_READER_ROLE_NAME));
                        }
                    }
                }

                foreach (var roleAssignment in roleAssignments)
                {
                    if (roleAssignment.Identity != null)
                    {
                        var alreadyExists = azDoEnvRoleAssignments.Exists(a => a.UserId == Guid.Parse(roleAssignment.Identity.LocalId));
                        if (!alreadyExists)
                        {
                            var customRole = customRoles.FirstOrDefault(ca => ca.Id.Equals(roleAssignment.CustomRoleId, StringComparison.OrdinalIgnoreCase));
                            if (customRole != null && customRole.EnvironmentRole != null && !string.IsNullOrWhiteSpace(customRole.EnvironmentRole.Name))
                            {
                                azDoEnvRoleAssignments.Add(new AzDoRoleAssignment(
                                    UserId: Guid.Parse(roleAssignment.Identity.LocalId),
                                    RoleName: customRole.EnvironmentRole.Name));
                            }
                        }
                    }
                }

                await DevOpsClient.AddEnvironmentRolesAsync(AzureDevOpsClientConfig.orgName, projectId, environmentId, azDoEnvRoleAssignments);

                logger.LogInformation($"Enforce security for {GetResourceKindAsString()}:{resourceId} completed.");
            }
        }

        protected override ResourceKind GetResourceKind() => ResourceKind.Environment;
    }
}
