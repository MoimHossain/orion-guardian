

using Microsoft.Extensions.Logging;
using Stellaris.Shared.AzureDevOps;
using Stellaris.Shared.Business.Abstractions;
using Stellaris.Shared.Config;
using Stellaris.Shared.Schemas;
using Stellaris.Shared.Storage;

namespace Stellaris.Shared.Business
{
    public class FolderPermissionsEvaluationService : BusinessServiceBase
    {
        private readonly AzureDevOpsClientConfig azureDevOpsClientConfig;
        private readonly SecurityCosmosClient securityCosmosClient;
        private readonly DevOpsClient devOpsClient;
        private readonly ILogger<SecurityEnforcementService> logger;

        public FolderPermissionsEvaluationService(
            AzureDevOpsClientConfig azureDevOpsClientConfig,
            SecurityCosmosClient securityCosmosClient,
            DevOpsClient devOpsClient,            
            ILogger<SecurityEnforcementService> logger) : base(azureDevOpsClientConfig, devOpsClient)
        {
            this.azureDevOpsClientConfig = azureDevOpsClientConfig;
            this.securityCosmosClient = securityCosmosClient;
            this.devOpsClient = devOpsClient;
            this.logger = logger;
        }

        
        public async Task<FolderSecurityEvaluationResult> GetEvaluatedFolderPermissionsAsync(
            string projectId, string folderId)
        {
            var userDescriptor = await GetCurrentUserDescriptorAsync();
            FolderSecurityEvaluationResult securityResult = new();
            var securityDescriptors = await securityCosmosClient
                .ListSecurityDescriptorsAsync(projectId, folderId, ResourceKind.Folder.ToString());

            if (securityDescriptors != null)
            {
                await EvaluateFolderPermissionsCore(userDescriptor, securityDescriptors, securityResult);
            }
            return securityResult;
        }

        public async Task<FolderSecurityEvaluationResult> GetEvaluatedFolderPermissionsAsync(
            string projectId, List<SecurityDescriptorEntity> securityDescriptors)
        {
            var userDescriptor = await GetCurrentUserDescriptorAsync();
            FolderSecurityEvaluationResult securityResult = new();

            if (securityDescriptors != null)
            {
                await EvaluateFolderPermissionsCore(userDescriptor, securityDescriptors, securityResult);
            }
            return securityResult;
        }

        public async Task<List<FolderWithPermissionEntity>> PerformSecurityTrimmingAsync(
            string projectId, List<FolderEntity> folders)
        {
            var securityTrimmedFolders = new List<FolderWithPermissionEntity>();
            if (folders != null && folders.Count > 0)
            {
                var userDescriptor = await GetCurrentUserDescriptorAsync();

                foreach (var folder in folders)
                {
                    var securityEvaluationResult = await EvaluateFolderPermissions(projectId, folder, userDescriptor);

                    if (securityEvaluationResult.IsReader || securityEvaluationResult.IsAdmin)
                    {
                        securityTrimmedFolders.Add(FolderWithPermissionEntity.From(folder, securityEvaluationResult));
                    }
                }
            }
            return securityTrimmedFolders;
        }



        private async Task<FolderSecurityEvaluationResult> EvaluateFolderPermissions(
            string projectId, FolderEntity folder, string userDescriptor)
        {
            FolderSecurityEvaluationResult securityResult = new FolderSecurityEvaluationResult();

            var securityDescriptors = await securityCosmosClient
                .ListSecurityDescriptorsAsync(projectId, folder.Id, ResourceKind.Folder.ToString());

            if (securityDescriptors != null)
            {
                await EvaluateFolderPermissionsCore(userDescriptor, securityDescriptors, securityResult);
            }
            return securityResult;
        }

        private async Task EvaluateFolderPermissionsCore(
            string userDescriptor,
            List<SecurityDescriptorEntity> securityDescriptors,
            FolderSecurityEvaluationResult securityResult)
        {
            foreach (var sd in securityDescriptors)
            {
                if (sd.RoleDescriptors != null)
                {
                    foreach (var rd in sd.RoleDescriptors)
                    {
                        await EvaluateGroupMembershipCore(userDescriptor, rd, securityResult);
                    }
                }
            }
        }

        private async Task EvaluateGroupMembershipCore(
            string userDescriptor,
            RoleDescriptor rd,
            FolderSecurityEvaluationResult securityResult)
        {
            if (rd.Identity == null || rd.RoleDefinition == null)
            {
                return;
            }

            if (!securityResult.IsAdmin
                && rd.RoleDefinition.Name.Equals(RoleDefinition.ADMINISTRATOR, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogInformation($"Evaluating {rd.Identity.DisplayName} for admin membership");
                var hasMembership = await devOpsClient
                    .CheckMembershipAsync(azureDevOpsClientConfig.orgName, userDescriptor, rd.Identity.SubjectDescriptor);
                if (hasMembership)
                {
                    securityResult.IsAdmin = true;
                    securityResult.IsContributor = true;
                    securityResult.IsReader = true;
                    securityResult.Memberships.Add(new FolderSecurityGroupMembershipEvaluationResult(
                        rd.RoleDefinition.Name, rd.Identity.DisplayName, rd.Identity.SubjectDescriptor));
                }
            }

            if (!securityResult.IsAdmin && !securityResult.IsContributor
               && rd.RoleDefinition.Name.Equals(RoleDefinition.Contributor, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogInformation($"Evaluating {rd.Identity.DisplayName} for Contributor/User membership");
                var hasMembership = await devOpsClient.CheckMembershipAsync(azureDevOpsClientConfig.orgName, userDescriptor, rd.Identity.SubjectDescriptor);
                if (hasMembership)
                {
                    securityResult.IsContributor = true;
                    securityResult.IsReader = true;
                    securityResult.Memberships.Add(new FolderSecurityGroupMembershipEvaluationResult(
                        rd.RoleDefinition.Name, rd.Identity.DisplayName, rd.Identity.SubjectDescriptor));
                }
            }

            if (!securityResult.IsAdmin && !securityResult.IsContributor && !securityResult.IsReader
                && rd.RoleDefinition.Name.Equals(RoleDefinition.READER, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogInformation($"Evaluating {rd.Identity.DisplayName} for reader membership");
                var hasMembership = await devOpsClient.CheckMembershipAsync(azureDevOpsClientConfig.orgName, userDescriptor, rd.Identity.SubjectDescriptor);
                if (hasMembership)
                {
                    securityResult.IsReader = true;
                    securityResult.Memberships.Add(new FolderSecurityGroupMembershipEvaluationResult(
                        rd.RoleDefinition.Name, rd.Identity.DisplayName, rd.Identity.SubjectDescriptor));
                }
            }
        }
    }
}
