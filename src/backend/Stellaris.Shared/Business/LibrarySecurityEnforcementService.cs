

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
    public class LibrarySecurityEnforcementService(
        AzureDevOpsClientConfig azureDevOpsClientConfig,
        BuiltInAccountService builtInAccountService,
        LinkCosmosClient linkCosmosClient,
        SecurityEnforcementCosmosClient securityEnforcementCosmosClient,
        ILogger<LibrarySecurityEnforcementService> logger,
        DevOpsClient devOpsClient) : ResourceSecurityEnforcementBase(azureDevOpsClientConfig, builtInAccountService,
            linkCosmosClient, securityEnforcementCosmosClient, devOpsClient)
    {
        private const string BUILTIN_LIBRARY_ADMINISTRATOR_ROLE_NAME = "Administrator";
        private const string BUILTIN_LIBRARY_READER_ROLE_NAME = "Reader";

        protected async override Task EnforceCoreAsync(
            string projectId, string folderId, string resourceId,
            IEnumerable<CustomRoleDefinitionEntity> customRoles,
            IEnumerable<CustomRoleAssignmentEntity> roleAssignments,
            IEnumerable<Identity> folderAdministrators,
            SecurityDescriptorTranslator translator)
        {
            if(Int32.TryParse(resourceId, out int libraryId))
            {
                // disable inheritance
                await DevOpsClient.ChangeLibraryInheritanceAsync(AzureDevOpsClientConfig.orgName, 
                    projectId, libraryId, inheritPermissions: false);

                // remove all existing permissions
                var existingRoleAssignments = await DevOpsClient
                    .ListLibraryRoleAssignmentsAsync(AzureDevOpsClientConfig.orgName, projectId, libraryId);

                if(existingRoleAssignments != null && existingRoleAssignments.Value != null && existingRoleAssignments.Value.Any())
                {
                    await DevOpsClient.RemoveLibraryRolesAsync(AzureDevOpsClientConfig.orgName, 
                        projectId, libraryId, existingRoleAssignments.Value.Select(era => era.Identity.Id));
                }

                List<AzDoRoleAssignment> azDoLibraryRoleAssignments = [];

                var pcaAdministrators = await AccountService.GetProjectCollectionAdministratorsAsync(projectId);
                if (pcaAdministrators != null && !string.IsNullOrWhiteSpace(pcaAdministrators.LocalId))
                {
                    azDoLibraryRoleAssignments.Add(new AzDoRoleAssignment(
                        UserId: Guid.Parse(pcaAdministrators.LocalId),
                        RoleName: BUILTIN_LIBRARY_ADMINISTRATOR_ROLE_NAME));
                }

                var projectAdministrators = await AccountService.GetProjectAdministratorsAsync(projectId);
                if (projectAdministrators != null && !string.IsNullOrWhiteSpace(projectAdministrators.LocalId))
                {
                    azDoLibraryRoleAssignments.Add(new AzDoRoleAssignment(
                        UserId: Guid.Parse(projectAdministrators.LocalId),
                        RoleName: BUILTIN_LIBRARY_ADMINISTRATOR_ROLE_NAME));
                }

                var projectCollectionBuildService = await AccountService.GetProjectCollectionBuildServiceAsync(projectId);
                if(projectCollectionBuildService != null && !string.IsNullOrWhiteSpace(projectCollectionBuildService.LocalId))
                {
                    azDoLibraryRoleAssignments.Add(new AzDoRoleAssignment(
                        UserId: Guid.Parse(projectCollectionBuildService.LocalId),
                        RoleName: BUILTIN_LIBRARY_READER_ROLE_NAME));
                }

                var projectBuildService = await AccountService.GetProjectBuildServiceAsync(projectId);
                if (projectBuildService != null && !string.IsNullOrWhiteSpace(projectBuildService.Id))
                {
                    azDoLibraryRoleAssignments.Add(new AzDoRoleAssignment(
                        UserId: Guid.Parse(projectBuildService.Id),
                        RoleName: BUILTIN_LIBRARY_READER_ROLE_NAME));
                }

                // Folder admins are automatically granted Reader role
                var folderAdminLocalIds = folderAdministrators
                    .Where(fa => !IsSubjectAlreadyConfigured(fa.SubjectDescriptor, roleAssignments))
                    .Select(fa => fa.LocalId);
                if (folderAdminLocalIds != null && folderAdminLocalIds.Any())
                {
                    foreach (var folderAdminLocalId in folderAdminLocalIds)
                    {
                        var alreadyExists = azDoLibraryRoleAssignments.Exists(a => a.UserId == Guid.Parse(folderAdminLocalId));
                        if (!alreadyExists)
                        {
                            azDoLibraryRoleAssignments.Add(new AzDoRoleAssignment(
                                UserId: Guid.Parse(folderAdminLocalId),
                                RoleName: BUILTIN_LIBRARY_READER_ROLE_NAME));
                        }
                    }
                }

                foreach (var roleAssignment in roleAssignments)
                {
                    if (roleAssignment.Identity != null)
                    {
                        var alreadyExists = azDoLibraryRoleAssignments
                            .Exists(a => a.UserId == Guid.Parse(roleAssignment.Identity.LocalId));
                        if (!alreadyExists)
                        {
                            var customRole = customRoles
                                .FirstOrDefault(ca => ca.Id.Equals(roleAssignment.CustomRoleId, StringComparison.OrdinalIgnoreCase));
                            if (customRole != null && customRole.LibraryRole != null
                                && !string.IsNullOrWhiteSpace(customRole.LibraryRole.Name))
                            {
                                azDoLibraryRoleAssignments.Add(new AzDoRoleAssignment(
                                    UserId: Guid.Parse(roleAssignment.Identity.LocalId),
                                    RoleName: customRole.LibraryRole.Name));
                            }
                        }
                    }
                }

                await DevOpsClient.AddLibraryRolesAsync(
                    AzureDevOpsClientConfig.orgName, projectId, libraryId, azDoLibraryRoleAssignments);

                logger.LogInformation($"Enforce security for {GetResourceKindAsString()}:{resourceId} completed.");
            }            
        }

        protected override ResourceKind GetResourceKind() => ResourceKind.Library;
    }
}
