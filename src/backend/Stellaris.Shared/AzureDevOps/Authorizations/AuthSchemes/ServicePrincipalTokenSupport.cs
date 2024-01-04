

using Azure.Core;
using Azure.Identity;
using Stellaris.Shared.Config;

namespace Stellaris.Shared.AzureDevOps.Authorizations.AuthSchemes
{
    public class ServicePrincipalTokenSupport
    {
        private readonly AzureDevOpsClientConfig appConfig;
        private TokenCredential? tokenCredential;

        public ServicePrincipalTokenSupport(AzureDevOpsClientConfig appConfig)
        {
            this.appConfig = appConfig;
            if (IsConfigured())
            {
                tokenCredential = new ClientSecretCredential(
                    appConfig.tenantIdOfServicePrincipal, 
                    appConfig.clientIdOfServicePrincipal, 
                    appConfig.clientSecretOfServicePrincipal);
            }
        }

        public async Task<(string, string)> GetCredentialsAsync()
        {
            if (tokenCredential == null)
            {
                throw new InvalidOperationException("Service Principal Authentication is not configured but attempting to use.");
            }

            // Whenever possible, credential instance should be reused for the lifetime of the process.
            // An internal token cache is used which reduces the number of outgoing calls to Azure AD to get tokens.
            // Call GetTokenAsync whenever you are making a request. Token caching and refresh logic is handled by the credential object.
            var tokenRequestContext = new TokenRequestContext(VssAadSettings.DefaultScopes);
            var accessToken = await tokenCredential.GetTokenAsync(tokenRequestContext, CancellationToken.None);
            return ("Bearer", accessToken.Token);
        }

        public bool IsConfigured()
        {
            var hasClientId = !string.IsNullOrWhiteSpace(appConfig.clientIdOfServicePrincipal);
            var hasClientSecret = !string.IsNullOrWhiteSpace(appConfig.clientSecretOfServicePrincipal);
            var hasTenantId = !string.IsNullOrWhiteSpace(appConfig.tenantIdOfServicePrincipal);

            return appConfig.useServicePrincipal && hasClientId && hasClientSecret && hasTenantId;
        }
    }
}
