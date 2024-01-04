

using Microsoft.AspNetCore.Http;
using Stellaris.Shared.AzureDevOps.Authorizations.AuthSchemes;
using Stellaris.Shared.Config;

namespace Stellaris.Shared.AzureDevOps.Authorizations
{
    // Transient in DI
    public class AuthorizationFactory
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ServicePrincipalTokenSupport servicePrincipalTokenSupport;
        private readonly PersonalAccessTokenSupport personalAccessTokenSupport;
        private readonly ManagedIdentityTokenSupport managedIdentityTokenSupport;
        private readonly AzureDevOpsClientConfig config;

        public AuthorizationFactory(
            IHttpContextAccessor httpContextAccessor,
            ServicePrincipalTokenSupport servicePrincipalTokenSupport,
            PersonalAccessTokenSupport personalAccessTokenSupport,
            ManagedIdentityTokenSupport managedIdentityTokenSupport,
            AzureDevOpsClientConfig config)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.servicePrincipalTokenSupport = servicePrincipalTokenSupport;
            this.personalAccessTokenSupport = personalAccessTokenSupport;
            this.managedIdentityTokenSupport = managedIdentityTokenSupport;
            this.config = config;
        }

        public async Task<(string, string)> GetCredentialsAsync(bool elevatedPrivilege = false)
        {
            if (elevatedPrivilege)
            {
                if (managedIdentityTokenSupport.IsConfigured())
                {
                    return await managedIdentityTokenSupport.GetCredentialsAsync();
                }
                if (servicePrincipalTokenSupport.IsConfigured())
                {
                    return await servicePrincipalTokenSupport.GetCredentialsAsync();
                }
                if (personalAccessTokenSupport.IsConfigured())
                {
                    return personalAccessTokenSupport.GetCredentials();
                }
            }
            else if (httpContextAccessor != null && httpContextAccessor.HttpContext != null)
            {
                var request = httpContextAccessor.HttpContext.Request;
                if (request.Headers.TryGetValue("Authorization", out var authInfo) && authInfo.Any())
                {
                    var authValue = authInfo.First();
                    var authValues = $"{authValue}".Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (authValues != null && authValues.Length > 0)
                    {
                        var scheme = authValues[0];
                        var token = authValues[1];
                        return (scheme, token);
                    }
                }
            }
            throw new UnauthorizedAccessException("Request is denied!");
        }
    }
}
