
using Stellaris.Shared.AzureDevOps;
using Stellaris.Shared.AzureDevOps.DTOs;
using Stellaris.Shared.Config;
using Stellaris.Shared.Storage;

namespace Stellaris.Shared.Business.Abstractions
{
    public abstract class BusinessServiceBase
    {
        private readonly AzureDevOpsClientConfig azureDevOpsClientConfig;        
        private readonly DevOpsClient devOpsClient;
        private readonly List<string> commonFields = ["DisplayName", "ScopeName", "Active", "SubjectDescriptor", "Description"];
        private readonly IdentitySearchPayloadOptions searchOption = new IdentitySearchPayloadOptions(1, 20);
        private readonly List<string> operationScopes = ["ims", "source"];

        protected BusinessServiceBase(
            AzureDevOpsClientConfig azureDevOpsClientConfig,
            DevOpsClient devOpsClient)
        {
            this.azureDevOpsClientConfig = azureDevOpsClientConfig;            
            this.devOpsClient = devOpsClient;
        }

        protected async Task<string> GetCurrentUserDescriptorAsync()
        {
            var connectionData = await devOpsClient.GetConnectionDataAsync(azureDevOpsClientConfig.orgName);
            var userDescriptor = connectionData.AuthenticatedUser.SubjectDescriptor;

            if (string.IsNullOrWhiteSpace(userDescriptor))
            {
                throw new UnauthorizedAccessException("Access denied");
            }
            return userDescriptor;
        }

        protected virtual async Task<AzDoIdentity?> LoadIdentityCoreAsync(string accountName, bool isGroup)
        {
            List<string> type = isGroup ? ["group"] : ["user"];
            var payload = new IdentitySearchPayload(accountName, type, operationScopes, searchOption, commonFields);
            var searchResult = await DevOpsClient.SearchIdentityAsync(AzureDevOpsClientConfig.orgName, payload);
            if (searchResult != null)
            {
                var identity = searchResult.FirstOrDefault();
                if (identity != null && !string.IsNullOrWhiteSpace(identity.SubjectDescriptor))
                {
                    return identity;
                }
            }
            return null;
        }

        protected DevOpsClient DevOpsClient => devOpsClient;
        protected AzureDevOpsClientConfig AzureDevOpsClientConfig => azureDevOpsClientConfig;
    }
}
