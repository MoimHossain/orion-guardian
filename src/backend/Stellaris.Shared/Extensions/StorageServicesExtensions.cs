

using Microsoft.Extensions.DependencyInjection;
using Stellaris.Shared.Config;
using Stellaris.Shared.Storage;

namespace Stellaris.Shared.Extensions
{
    public static class StorageServicesExtensions
    {
        public static IServiceCollection AddStorageServices(this IServiceCollection services)
        {
            services.AddSingleton(ConfigHelper.GetCosmosDbConfig());

            services.AddSingleton<FolderCosmosClient>();
            services.AddSingleton<LinkCosmosClient>();
            services.AddSingleton<GeneralCosmosClient>();
            services.AddSingleton<SecurityCosmosClient>();
            services.AddSingleton<AuditCosmosClient>();
            services.AddSingleton<CustomRoleCosmosClient>();
            services.AddSingleton<CustomRoleAssignmentCosmosClient>();
            services.AddSingleton<SecurityEnforcementCosmosClient>();
            services.AddSingleton<ChangeEventCosmosClient>();
            services.AddSingleton<FeedProcessorCosmosClient>();

            return services;
        }
    }
}
