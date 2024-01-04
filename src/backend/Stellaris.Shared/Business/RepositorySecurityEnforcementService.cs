

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
    public class RepositorySecurityEnforcementService(
        AzureDevOpsClientConfig azureDevOpsClientConfig,
        BuiltInAccountService builtInAccountService,
        LinkCosmosClient linkCosmosClient,
        SecurityEnforcementCosmosClient securityEnforcementCosmosClient,
        ILogger<RepositorySecurityEnforcementService> logger,
        DevOpsClient devOpsClient) : ResourceSecurityEnforcementBase(azureDevOpsClientConfig, builtInAccountService, 
            linkCosmosClient, securityEnforcementCosmosClient, devOpsClient)
    {
        private const string REPOSITORY_SECURITY_TOKEN = "repoV2";
        private const string REPOSITORY_SECURITY_NAMESPACE_ID = "2e9eb7ed-3c0a-47d4-87c1-0ffdd275fd87";
        private const int BIT_REPOSITORY_READ = 2;

        protected override ResourceKind GetResourceKind() => ResourceKind.Repository;

        protected async override Task EnforceCoreAsync(
            string projectId, string folderId, string resourceId,
            IEnumerable<CustomRoleDefinitionEntity> customRoles,
            IEnumerable<CustomRoleAssignmentEntity> roleAssignments,
            IEnumerable<Identity> folderAdministrators, SecurityDescriptorTranslator translator)
        {
            List<AzDoAclEntryCollection> totalAccessControlEntries = [];

            var translatedDescriptors = await GetBuiltInAccountDescriptorsAsync(projectId, roleAssignments,
                folderAdministrators, translator);
            if (translatedDescriptors != null)
            {
                var builtInAclEntries = translatedDescriptors
                    .Select(td => AzDoAclDictionaryEntry.GeneratePermission(td, BIT_REPOSITORY_READ));

                totalAccessControlEntries.Add(new AzDoAclEntryCollection(
                    InheritPermissions: false,
                    Token: $"{REPOSITORY_SECURITY_TOKEN}/{projectId}/{resourceId}",
                    AcesDictionary: builtInAclEntries.ToDictionary(x => x.Descriptor, x => x)));
            }

            var customAccessControlEntries = GenerateAclPayload(projectId, resourceId, roleAssignments, customRoles, translator);
            if (customAccessControlEntries != null && customAccessControlEntries.Count > 0)
            {
                totalAccessControlEntries.AddRange(customAccessControlEntries);
            }
            await DevOpsClient.ApplyAclsAsync(AzureDevOpsClientConfig.orgName, REPOSITORY_SECURITY_NAMESPACE_ID, [.. totalAccessControlEntries]);

            logger.LogInformation($"Enforce security for {GetResourceKindAsString()}:{resourceId} completed.");
        }



        private async Task<IEnumerable<string>> GetBuiltInAccountDescriptorsAsync(
            string projectId,
            IEnumerable<CustomRoleAssignmentEntity> roleAssignments,
            IEnumerable<Identity> folderAdministrators,
            SecurityDescriptorTranslator translator)
        {
            var subjectDescriptorsToTranslate = new List<string>();

            var folderAdminSDs = folderAdministrators
                .Where(fa => !IsSubjectAlreadyConfigured(fa.SubjectDescriptor, roleAssignments))
                .Select(fa => fa.SubjectDescriptor);

            subjectDescriptorsToTranslate.AddRange(folderAdminSDs);

            var pcaBuildService = await AccountService.GetProjectCollectionBuildServiceAsync(projectId);
            if (pcaBuildService != null && !string.IsNullOrWhiteSpace(pcaBuildService.SubjectDescriptor))
            {
                subjectDescriptorsToTranslate.Add(pcaBuildService.SubjectDescriptor);
            }

            var pcaAdministrators = await AccountService.GetProjectCollectionAdministratorsAsync(projectId);
            if (pcaAdministrators != null && !string.IsNullOrWhiteSpace(pcaAdministrators.SubjectDescriptor))
            {
                subjectDescriptorsToTranslate.Add(pcaAdministrators.SubjectDescriptor);
            }

            var projectAdministrators = await AccountService.GetProjectAdministratorsAsync(projectId);
            if (projectAdministrators != null && !string.IsNullOrWhiteSpace(projectAdministrators.SubjectDescriptor))
            {
                subjectDescriptorsToTranslate.Add(projectAdministrators.SubjectDescriptor);
            }

            await translator.IncludeAsync(subjectDescriptorsToTranslate);

            var translatedDescriptors = new List<string>(
                subjectDescriptorsToTranslate
                .Select(sd => translator.Translate(sd))
                .Where(td => !string.IsNullOrWhiteSpace(td)));

            var projectBuildService = await AccountService.GetProjectBuildServiceAsync(projectId);
            if (projectBuildService != null && !string.IsNullOrWhiteSpace(projectBuildService.Descriptor))
            {
                translatedDescriptors.Add(projectBuildService.Descriptor);
            }
            return translatedDescriptors.Distinct();
        }

        private List<AzDoAclEntryCollection> GenerateAclPayload(
            string projectId, string resourceId,
            IEnumerable<CustomRoleAssignmentEntity> roleAssignments,
            IEnumerable<CustomRoleDefinitionEntity> customRoles,
            SecurityDescriptorTranslator translator)
        {
            var aclCollections = new List<AzDoAclEntryCollection>();
            if (customRoles != null && roleAssignments != null)
            {
                var aclEntries = GetAclEntriesFromRoleAssignments(translator, roleAssignments, customRoles);
                if (aclEntries != null && aclEntries.Count > 0)
                {
                    var aclCollection = new AzDoAclEntryCollection(
                        InheritPermissions: false,
                        Token: $"{REPOSITORY_SECURITY_TOKEN}/{projectId}/{resourceId}",
                        AcesDictionary: aclEntries.ToDictionary(x => x.Descriptor, x => x));
                    aclCollections.Add(aclCollection);
                }
            }
            return aclCollections;
        }

        private List<AzDoAclDictionaryEntry> GetAclEntriesFromRoleAssignments(
            SecurityDescriptorTranslator translator,
            IEnumerable<CustomRoleAssignmentEntity> roleAssignments,
            IEnumerable<CustomRoleDefinitionEntity> customRoles)
        {
            var aclEntries = new List<AzDoAclDictionaryEntry>();
            foreach (var roleAssignment in roleAssignments)
            {
                if (roleAssignment.Identity != null)
                {
                    var customRole = customRoles.Where(x => x.Id == roleAssignment.CustomRoleId).FirstOrDefault();
                    if (customRole != null)
                    {
                        var translatedDescriptor = translator.Translate(roleAssignment.Identity.SubjectDescriptor);
                        if (!string.IsNullOrWhiteSpace(translatedDescriptor))
                        {
                            var aclEntry = customRole.RepositoryPermissionGrants.GenerateAclFromRole(translatedDescriptor);
                            aclEntries.Add(aclEntry);
                        }
                    }
                }
            }
            return aclEntries;
        }
    }
}
