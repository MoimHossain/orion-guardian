

using Microsoft.Extensions.DependencyInjection;
using Stellaris.Shared.Business;

namespace Stellaris.Shared.Extensions
{
    public static class BusinessServicesExtensions
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            services.AddSingleton<OrganizationService>();
            services.AddSingleton<BuiltInAccountService>();
            services.AddSingleton<FolderPermissionsEvaluationService>();
            services.AddSingleton<SecurityEnforcementService>();
            services.AddSingleton<RepositorySecurityEnforcementService>();
            services.AddSingleton<EnvironmentSecurityEnforcementService>();
            services.AddSingleton<ServiceEndpointSecurityEnforcementService>();
            services.AddSingleton<LibrarySecurityEnforcementService>();
            services.AddSingleton<ChangeEventProcessingService>();

            return services;
        }
    }
}
