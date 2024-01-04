


using Stellaris.Shared.Schemas;

namespace Stellaris.Shared.AzureDevOps.DTOs
{
    public static class HandyExtensions
    {
        public static AzDoAclDictionaryEntry GenerateAclFromRole(this IEnumerable<PermissionGrant> permissionGrants, string descriptor)
        {
            int allow = 0;
            int deny = 0;
            foreach (var permissionGrant in permissionGrants)
            {
                if (permissionGrant.Grant == GrantType.Allow)
                {
                    allow |= permissionGrant.Bit;
                }
                if (permissionGrant.Grant == GrantType.Deny)
                {
                    deny |= permissionGrant.Bit;
                }
            }
            var aclEntry = new AzDoAclDictionaryEntry(descriptor, allow, deny);
            return aclEntry;
        }
    }
}
