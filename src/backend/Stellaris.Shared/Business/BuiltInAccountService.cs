

using Stellaris.Shared.AzureDevOps;
using Stellaris.Shared.AzureDevOps.DTOs;
using Stellaris.Shared.Business.Abstractions;
using Stellaris.Shared.Config;
using Stellaris.Shared.Schemas;

namespace Stellaris.Shared.Business
{
    public class BuiltInAccountService(
        AzureDevOpsClientConfig azureDevOpsClientConfig,
        OrganizationService organizationService,
        DevOpsClient devOpsClient) : BusinessServiceBase(azureDevOpsClientConfig, devOpsClient)
    {
        public async Task<AzdoIdentitySlim?> GetProjectBuildServiceAsync(string projectId)
        {
            var organizationId = await organizationService.GetAzureDevOpsOrganizationId();
            var projectBuildServiceAccountDescriptor = $"Microsoft.TeamFoundation.ServiceIdentity;{organizationId}:Build:{projectId}";
            var projectBuildService = await DevOpsClient
                .GetIdentityByDescriptorAsync(AzureDevOpsClientConfig.orgName, projectBuildServiceAccountDescriptor);
            return projectBuildService;
        }

        public async Task<AzDoIdentity?> GetProjectCollectionBuildServiceAsync(string projectId)
        {
            var projectCollectionBuildService = $"{BuiltInAccountType.ProjectCollectionBuildService} ({AzureDevOpsClientConfig.orgName})";
            var pcaBuildService = await LoadIdentityCoreAsync(projectCollectionBuildService, false);
            return pcaBuildService;
        }

        public async Task<AzDoIdentity?> GetProjectCollectionAdministratorsAsync(string projectId)
        {
            var projectCollectionAdministrators = $"[{AzureDevOpsClientConfig.orgName}]\\{BuiltInAccountType.ProjectCollectionAdministrators}";
            var pcaAdministrators = await LoadIdentityCoreAsync(projectCollectionAdministrators, true);
            return pcaAdministrators;
        }

        public async Task<AzDoIdentity?> GetProjectAdministratorsAsync(string projectId)
        {
            var project = await DevOpsClient.GetProjectAsync(AzureDevOpsClientConfig.orgName, projectId);

            var projectAdministrators = $"[{project.Name}]\\{BuiltInAccountType.ProjectAdministrators}";
            var projectAdministratorsIdentity = await LoadIdentityCoreAsync(projectAdministrators, true);
            return projectAdministratorsIdentity;
        }

        public async Task<BuiltInUserPrivilege> GetCurrentUserBuiltInRolesAsync(string projectId)
        {
            var isProjectCollectionAdmin = false;
            var isProjectAdmin = false;

            var currentUserDescriptor = await GetCurrentUserDescriptorAsync();
            var projectCollectionAdmins = await GetProjectCollectionAdministratorsAsync(projectId);
            if(projectCollectionAdmins != null && !string.IsNullOrWhiteSpace(projectCollectionAdmins.SubjectDescriptor))
            {
                isProjectCollectionAdmin = await DevOpsClient
                    .CheckMembershipAsync(AzureDevOpsClientConfig.orgName, 
                    currentUserDescriptor, projectCollectionAdmins.SubjectDescriptor);
            }

            var projectAdmins = await GetProjectAdministratorsAsync(projectId);
            if(projectAdmins != null && !string.IsNullOrWhiteSpace(projectAdmins.SubjectDescriptor))
            {
                isProjectAdmin = await DevOpsClient.CheckMembershipAsync(AzureDevOpsClientConfig.orgName, 
                    currentUserDescriptor, projectAdmins.SubjectDescriptor);
            }

            return new BuiltInUserPrivilege((isProjectCollectionAdmin == true || isProjectAdmin == true), 
                isProjectCollectionAdmin, isProjectAdmin);
        }
    }
}
