

using Microsoft.Extensions.DependencyInjection;
using Stellaris.Shared.AzureDevOps;
using Stellaris.Shared.AzureDevOps.Authorizations;
using Stellaris.Shared.AzureDevOps.Authorizations.AuthSchemes;
using Stellaris.Shared.Config;
using System.Text.Json;

namespace Stellaris.Shared.Extensions
{
    public static class AzureDevOpsServiceExtensions
    {
        public static IServiceCollection AddAzureDevOpsClientServices(this IServiceCollection services)
        {
            services.AddSingleton(ConfigHelper.GetAzureDevOpsClientConfig());
            services.AddSingleton(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true
            });

            services.AddSingleton<PersonalAccessTokenSupport>();
            services.AddSingleton<ServicePrincipalTokenSupport>();
            services.AddSingleton<ManagedIdentityTokenSupport>();
            services.AddHttpClient(AzureDevOpsClientConstants.CoreAPI.NAME,
                (services, client) => { client.BaseAddress = new Uri(AzureDevOpsClientConstants.CoreAPI.URI); });
            services.AddHttpClient(AzureDevOpsClientConstants.VSSPS_API.NAME,
                (services, client) => { client.BaseAddress = new Uri(AzureDevOpsClientConstants.VSSPS_API.URI); });

            // NOTE: transient services
            services.AddHttpContextAccessor();
            services.AddTransient<AuthorizationFactory>();
            services.AddTransient<DevOpsClient>();
            return services;
        }
    }
}
