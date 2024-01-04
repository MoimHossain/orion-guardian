

using Microsoft.Extensions.Logging;
using Stellaris.Shared.AzureDevOps;
using Stellaris.Shared.AzureDevOps.Support;
using Stellaris.Shared.Business.Abstractions;
using Stellaris.Shared.Config;
using Stellaris.Shared.Extensions;
using Stellaris.Shared.Schemas;
using Stellaris.Shared.Storage;

namespace Stellaris.Shared.Business
{
    public class SecurityEnforcementService(
        AzureDevOpsClientConfig azureDevOpsClientConfig,
        SecurityCosmosClient securityCosmosClient,
        DevOpsClient devOpsClient,
        FolderPermissionsEvaluationService folderPermissionsEvaluationService,
        CustomRoleCosmosClient customRoleCosmosClient,
        CustomRoleAssignmentCosmosClient customRoleAssignmentCosmosClient,
        RepositorySecurityEnforcementService repositorySecurityEnforcementService,
        EnvironmentSecurityEnforcementService environmentSecurityEnforcementService,
        ServiceEndpointSecurityEnforcementService serviceEndpointSecurityEnforcementService,
        LibrarySecurityEnforcementService librarySecurityEnforcementService,
        ILogger<SecurityEnforcementService> logger) : BusinessServiceBase(azureDevOpsClientConfig, devOpsClient)
    {
        public async Task EnforceBatchFromDaemonServiceAsync(string projectId, string folderId)
        {
            ArgumentNullException.ThrowIfNull(projectId, nameof(projectId));
            ArgumentNullException.ThrowIfNull(folderId, nameof(folderId));

            var folderSecurityDescriptors = await securityCosmosClient
                .ListSecurityDescriptorsAsync(projectId, folderId, ResourceKind.Folder.ToString());            
            await EnforceBatchSecurityCoreAsync(projectId, folderId, folderSecurityDescriptors);
        }

        private async Task EnforceBatchSecurityCoreAsync(
            string projectId, string folderId, List<SecurityDescriptorEntity> folderSecurityDescriptors)
        {
            var customRoles = await customRoleCosmosClient.ListCustomRolesAsync(projectId);
            var roleAssignments = await customRoleAssignmentCosmosClient.ListCustomRolesAssignmentsAsync(projectId, folderId);
            var folderAdministrators = folderSecurityDescriptors.GetAdministrators();
            var translator = await SecurityDescriptorTranslator.FromAsync(roleAssignments, DevOpsClient, AzureDevOpsClientConfig);

            await librarySecurityEnforcementService.EnforceAsync(projectId, folderId,
                customRoles, roleAssignments, folderAdministrators, translator);

            await serviceEndpointSecurityEnforcementService.EnforceAsync(projectId, folderId,
                customRoles, roleAssignments, folderAdministrators, translator);
            await repositorySecurityEnforcementService.EnforceAsync(projectId, folderId,
                customRoles, roleAssignments, folderAdministrators, translator);
            await environmentSecurityEnforcementService.EnforceAsync(projectId, folderId,
                customRoles, roleAssignments, folderAdministrators, translator);

            logger.LogInformation($"Enforce security for {folderId} in background completed.");
        }

        private async Task ThrowIfCurrentUserIsUnauthorized(
            string projectId, List<SecurityDescriptorEntity> folderSecurityDescriptors)
        {
            var currentUserPrivilege = await folderPermissionsEvaluationService
                .GetEvaluatedFolderPermissionsAsync(projectId, folderSecurityDescriptors);
            if (!currentUserPrivilege.IsAdmin)
            {
                // Only admins are allowed to initiate enforcement
                throw new UnauthorizedAccessException("Access denied");
            }
        }
    }
}
