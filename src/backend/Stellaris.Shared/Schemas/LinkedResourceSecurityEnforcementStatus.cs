

using System.Text.Json.Serialization;

namespace Stellaris.Shared.Schemas
{
    public class LinkedResourceSecurityEnforcementStatus
    {
        [JsonPropertyName("lastEnforcedAt")]
        public DateTime LastEnforcedAt { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("enforcementStatus")]
        public string EnforcementStatus { get; set; } = string.Empty;
    }
}
