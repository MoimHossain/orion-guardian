using System.Text.Json.Serialization;

namespace Stellaris.Shared.Schemas
{
    public class PermissionGrant: Privilege
    {
        [JsonPropertyName("grant")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GrantType Grant { get; set; } = GrantType.NotSet;

    }

    public enum GrantType
    {
        NotSet,
        Allow,
        Deny
    }
}
