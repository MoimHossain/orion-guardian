

using Stellaris.Shared.AzureDevOps;
using Stellaris.Shared.AzureDevOps.DTOs;
using Stellaris.Shared.Business.Abstractions;
using Stellaris.Shared.Config;
using Stellaris.Shared.Storage;

namespace Stellaris.Shared.Business
{
    public class OrganizationService(
        AzureDevOpsClientConfig azureDevOpsClientConfig,
        DevOpsClient devOpsClient) : BusinessServiceBase(azureDevOpsClientConfig, devOpsClient)
    {
        private readonly List<string> commonFields = ["DisplayName", "ScopeName", "Active", "SubjectDescriptor", "Description"];
        private readonly IdentitySearchPayloadOptions searchOption = new IdentitySearchPayloadOptions(1, 20);
        private readonly List<string> operationScopes = ["ims", "source"];
        private readonly Dictionary<string, string> cachedOrgIds = [];

        public async Task<string> GetAzureDevOpsOrganizationId()
        {
            if (cachedOrgIds.TryGetValue(AzureDevOpsClientConfig.orgName, out string? value))
            {
                return value;
            }
            else
            {                
                var projectCollectionBuildService = $"{BuiltInAccountType.ProjectCollectionBuildService} ({AzureDevOpsClientConfig.orgName})";
                var identity = await LoadIdentityCoreAsync(projectCollectionBuildService, false);
                if (identity != null && !string.IsNullOrWhiteSpace(identity.SubjectDescriptor))
                {
                    var translatedIdentity = await DevOpsClient
                        .TranslateDescriptorsAsync(AzureDevOpsClientConfig.orgName, identity.SubjectDescriptor);
                    if (translatedIdentity != null && translatedIdentity.Count > 0)
                    {
                        var translatedId = translatedIdentity.First();
                        if (translatedId != null)
                        {
                            var descriptor = translatedId.Descriptor;
                            var firstIndexOfSemicolon = descriptor.IndexOf(";");
                            var firstIndexOfColon = descriptor.IndexOf(":");
                            if (firstIndexOfColon > firstIndexOfSemicolon)
                            {
                                var orgId = descriptor.Substring(firstIndexOfSemicolon + 1, firstIndexOfColon - firstIndexOfSemicolon - 1);
                                cachedOrgIds.Add(AzureDevOpsClientConfig.orgName, orgId);
                                return orgId;
                            }
                        }
                    }
                }
            }
            return string.Empty;
        }
    }
}
