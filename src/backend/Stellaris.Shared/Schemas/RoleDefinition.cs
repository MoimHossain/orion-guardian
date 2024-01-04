
using System.Text.Json.Serialization;

namespace Stellaris.Shared.Schemas
{
    // Do not change, the schema follows the AZDO API
    public class RoleDefinition : ICanValidate
    {
        public const string ADMINISTRATOR = "Administrator";
        public const string READER = "Reader";
        public const string Contributor = "User";

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("allowPermissions")]
        public int AllowPermissions { get; set; } = 0;

        [JsonPropertyName("denyPermissions")]
        public int DenyPermissions { get; set; } = 0;

        [JsonPropertyName("identifier")]
        public string Identifier { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;

        public bool IsValid()
        {
            if(string.IsNullOrWhiteSpace(DisplayName))
                return false;
            if(string.IsNullOrWhiteSpace(Name))
                return false;
            //if(AllowPermissions == 0)
            //    return false;
            //if(DenyPermissions == 0)
            //    return false;
            if(string.IsNullOrWhiteSpace(Identifier))
                return false;
            if(string.IsNullOrWhiteSpace(Description))
                return false;
            if(string.IsNullOrWhiteSpace(Scope))
                return false;
            return true;
        }
    }
}
