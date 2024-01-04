
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Stellaris.Shared.Config;

namespace Stellaris.Shared.AzureDevOps.Authorizations.AuthSchemes
{
    public class ManagedIdentityTokenSupport
    {
        private readonly AzureDevOpsClientConfig appConfig;
        private readonly ILogger<ManagedIdentityTokenSupport> logger;

        // Credentials object can be static or Singleton (as the case here)
        // so it can be reused across multiple requests. This ensures
        // the internal token cache is used which reduces the number of outgoing calls to Azure AD to get tokens.
        private TokenCredential? tokenCredential;

        public ManagedIdentityTokenSupport(AzureDevOpsClientConfig appConfig, ILogger<ManagedIdentityTokenSupport> logger)
        {
            this.appConfig = appConfig;
            this.logger = logger;
            if (this.IsConfigured())
            {
                logger.LogInformation("Managed Identity Auth is configured.");
                logger.LogInformation($"Managed Identity tenant ID = {appConfig.tenantIdOfManagedIdentity}");
                logger.LogInformation($"Managed Identity Client ID = {appConfig.clientIdOfManagedIdentity}");
                var option = new DefaultAzureCredentialOptions
                {
                    TenantId = appConfig.tenantIdOfManagedIdentity,
                    ManagedIdentityClientId = appConfig.clientIdOfManagedIdentity,
                    ExcludeEnvironmentCredential = true // Excluding because EnvironmentCredential was not using correct identity when running in Visual Studio
                };
                tokenCredential = new DefaultAzureCredential(option);
            }
        }


        public async Task<(string, string)> GetCredentialsAsync()
        {
            if (tokenCredential == null)
            {
                throw new InvalidOperationException("Managed Identity Auth is not configured but attempting to use.");
            }

            logger.LogInformation("Managed Identity Auth attempting to get token...");
            var tokenRequestContext = new TokenRequestContext(VssAadSettings.DefaultScopes);
            var accessToken = await tokenCredential.GetTokenAsync(tokenRequestContext, CancellationToken.None);
            logger.LogInformation("Managed Identity Auth is configured...token received");
            return ("Bearer", accessToken.Token);
        }

        public bool IsConfigured()
        {
            return appConfig.useManagedIdentity;
        }
    }
}
