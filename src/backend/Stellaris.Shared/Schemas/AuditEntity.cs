using System.Text.Json.Serialization;

namespace Stellaris.Shared.Schemas
{
    public class AuditEntity
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("projectId")]
        public string ProjectId { get; set; } = string.Empty;

        [JsonPropertyName("createdOn")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("properties")]
        public Dictionary<string, object>? Properties { get; set; }
    }
}
