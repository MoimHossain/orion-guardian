
using System.Text.Json.Serialization;

namespace Stellaris.Shared.Schemas
{
    public class SecurityEnforcementLog
    {
        [JsonPropertyName("partitionKey")]
        public string PartitionKey 
        { 
            get 
            { 
                return GetPartitionKey(this.FolderId, this.ResourceId);
            }
            set 
            { 
                // ignore
            } 
        }

        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("projectId")]
        public string ProjectId { get; set; } = string.Empty;

        [JsonPropertyName("folderId")]
        public string FolderId { get; set; } = string.Empty;

        [JsonPropertyName("resourceId")]
        public string ResourceId { get; set; } = string.Empty;

        [JsonPropertyName("resourceKind")]
        public string ResourceKind { get; set; } = string.Empty;

        [JsonPropertyName("lastEnforcedAt")]
        public DateTime LastEnforcedAt { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("enforcementStatus")]
        public EnforcementStatus EnforcementStatus { get; set; } = EnforcementStatus.NOT_ENFORCED;

        public static string GetPartitionKey(string folderId, string resourceId)
        {
            return $"{folderId}_{resourceId}".ToUpperInvariant();
        }
    }

    public enum EnforcementStatus
    {
        ENFORCED,
        IN_PROGRESS,
        NOT_ENFORCED
    }
}
