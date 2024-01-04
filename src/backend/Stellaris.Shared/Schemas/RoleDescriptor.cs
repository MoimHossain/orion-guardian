using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Stellaris.Shared.Schemas
{
    public class RoleDescriptor : ICanValidate
    {
        [JsonPropertyName("identity")]
        public Identity? Identity { get; set; }

        [JsonPropertyName("roleDefinition")]
        public RoleDefinition RoleDefinition { get; set; } = new RoleDefinition();

        public bool IsValid()
        {
            if(Identity == null)
                return false;
            if(RoleDefinition == null)
                return false;
            if(Identity.IsValid() == false)
                return false;
            if(RoleDefinition.IsValid() == false)
                return false;
            return true;
        }
    }
}
