

using Microsoft.Extensions.DependencyInjection;
using Stellaris.Shared.Config;
using Stellaris.Shared.Schemas;
using Stellaris.Shared.Storage;

namespace Stellaris.Shared.Extensions
{
    public static class CommonExtensions
    {
        public static IServiceCollection AddServicesFromSharedLibrary(this IServiceCollection services)
        {
            services.AddAzureDevOpsClientServices();
            services.AddBusinessServices();
            services.AddStorageServices();

            return services;
        }

        public static List<Identity> GetAdministrators(this List<SecurityDescriptorEntity> securityDescriptorEntities)
        {
            var administrators = new List<Identity>();
            if (securityDescriptorEntities != null)
            {
                foreach (var securityDescriptorEntity in securityDescriptorEntities)
                {
                    if (securityDescriptorEntity.RoleDescriptors != null)
                    {
                        foreach (var roleDescriptor in securityDescriptorEntity.RoleDescriptors)
                        {
                            if (roleDescriptor.Identity != null &&
                                roleDescriptor.RoleDefinition.Name.Equals(RoleDefinition.ADMINISTRATOR, StringComparison.OrdinalIgnoreCase))
                            {
                                administrators.Add(roleDescriptor.Identity);
                            }
                        }
                    }
                }
            }
            return administrators;
        }
    }
}
