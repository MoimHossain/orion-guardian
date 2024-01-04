

using System.Text.Json.Serialization;

namespace Stellaris.Shared.Schemas
{
    public class LinkEntity
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("kind")]
        public string Kind { get; set; } = string.Empty;

        [JsonPropertyName("folderId")]
        public string FolderId { get; set; } = string.Empty;

        [JsonPropertyName("resourceId")]
        public string ResourceId { get; set; } = string.Empty;

        [JsonPropertyName("resourceName")]
        public string ResourceName { get; set; } = string.Empty;

        [JsonPropertyName("projectId")]
        public string ProjectId { get; set; } = string.Empty;

        [JsonPropertyName("lastModified")]
        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("createdOn")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("createdBy")]
        public User? CreatedBy { get; set; }

        [JsonPropertyName("lastModifiedBy")]
        public User? LastModifiedBy { get; set; }

        [JsonPropertyName("enforcementStatus")]
        public LinkedResourceSecurityEnforcementStatus? EnforcementStatus { get; set; }
    }
}
