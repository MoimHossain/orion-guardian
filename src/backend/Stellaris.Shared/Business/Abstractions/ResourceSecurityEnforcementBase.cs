

using Stellaris.Shared.AzureDevOps;
using Stellaris.Shared.AzureDevOps.Support;
using Stellaris.Shared.Config;
using Stellaris.Shared.Schemas;
using Stellaris.Shared.Storage;
using Identity = Stellaris.Shared.Schemas.Identity;

namespace Stellaris.Shared.Business.Abstractions
{
    public abstract class ResourceSecurityEnforcementBase(
        AzureDevOpsClientConfig azureDevOpsClientConfig,
        BuiltInAccountService builtInAccountService,
        LinkCosmosClient linkCosmosClient,
        SecurityEnforcementCosmosClient securityEnforcementCosmosClient,
        DevOpsClient devOpsClient) : BusinessServiceBase(azureDevOpsClientConfig, devOpsClient)
    {
        private readonly BuiltInAccountService builtInAccountService = builtInAccountService;
        protected BuiltInAccountService AccountService { get => builtInAccountService; }

        public async Task<IEnumerable<SecurityEnforcementLog>> EnforceAsync(
            string projectId, string folderId,
            IEnumerable<CustomRoleDefinitionEntity> customRoles,
            IEnumerable<CustomRoleAssignmentEntity> roleAssignments,
            IEnumerable<Identity> folderAdministrators, SecurityDescriptorTranslator translator)
        {
            ArgumentNullException.ThrowIfNull(projectId, nameof(projectId));
            ArgumentNullException.ThrowIfNull(folderId, nameof(folderId));

            var enforcementLogs = new List<SecurityEnforcementLog>();
            var linkedResources = await linkCosmosClient.ListLinksByFolderIdAsync(projectId, GetResourceKindAsString(), folderId);
            if (linkedResources != null && linkedResources.Count != 0)
            {
                foreach (var linkedResource in linkedResources)
                {
                    var eLog = await EnforceTemplateMethodAsync(projectId, folderId, 
                        linkedResource.ResourceId, linkedResource,
                        customRoles, roleAssignments, folderAdministrators, translator);
                    if(eLog != null)
                    {
                        enforcementLogs.Add(eLog);
                    }
                }
            }
            return enforcementLogs;
        }

        protected async virtual Task<SecurityEnforcementLog> EnforceTemplateMethodAsync(
            string projectId, string folderId, string resourceId,
            LinkEntity link,
            IEnumerable<CustomRoleDefinitionEntity> customRoles,
            IEnumerable<CustomRoleAssignmentEntity> roleAssignments,
            IEnumerable<Identity> folderAdministrators,
            SecurityDescriptorTranslator translator)
        {
            _ = await securityEnforcementCosmosClient
                .CreateIfNotExistedAsync(projectId, folderId, resourceId, GetResourceKindAsString());

            await EnforceCoreAsync(projectId, folderId, resourceId, customRoles, roleAssignments, folderAdministrators, translator);

            await linkCosmosClient.UpdateSecurityEnforcementStampAsync(projectId, link.Id, EnforcementStatus.ENFORCED);

            var enforceLog = await securityEnforcementCosmosClient.UpdateEnforcementStatusAsync(
                projectId, folderId, resourceId, GetResourceKindAsString(), EnforcementStatus.ENFORCED);
            return enforceLog;
        }

        protected abstract Task EnforceCoreAsync(
            string projectId, string folderId, string resourceId,
            IEnumerable<CustomRoleDefinitionEntity> customRoles,
            IEnumerable<CustomRoleAssignmentEntity> roleAssignments,
            IEnumerable<Identity> folderAdministrators, SecurityDescriptorTranslator translator);

        protected bool IsSubjectAlreadyConfigured(string subjectDescriptor, IEnumerable<CustomRoleAssignmentEntity> roleAssignments)
        {
            foreach (var roleAssignment in roleAssignments)
            {
                if (roleAssignment.Identity != null)
                {
                    if (roleAssignment.Identity.SubjectDescriptor.Equals(subjectDescriptor, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected abstract ResourceKind GetResourceKind();

        protected string GetResourceKindAsString() => GetResourceKind().ToString();
    }
}
