
using System.Text.Json.Serialization;

namespace Stellaris.Shared.Schemas
{
    public class PermissionDescriptor : ICanValidate
    {
        [JsonPropertyName("identity")]
        public Identity? Identity { get; set; }

        [JsonPropertyName("permissions")]
        public List<PermissionGrant> Permissions { get; set; } = new List<PermissionGrant>();

        public bool IsValid()
        {
            if(Identity == null)
                return false;
            if(Permissions == null)
                return false;
            if(Identity.IsValid() == false)
                return false;
            if(Permissions.Count == 0)
                return false;
            foreach(var grant in Permissions)
            {
                if(grant.IsValid() == false)
                    return false;
            }
            return true;
        }
    }
}
