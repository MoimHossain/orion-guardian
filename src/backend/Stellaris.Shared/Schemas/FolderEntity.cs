using System.Text.Json.Serialization;

namespace Stellaris.Shared.Schemas
{
    public class FolderEntity
    {
        [JsonPropertyName("projectId")]
        public string ProjectId { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("kind")]
        public string Kind { get; set; } = string.Empty;

        [JsonPropertyName("parentFolderId")]
        public string ParentFolderId { get; set; } = string.Empty;

        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("ciNumber")]
        public string CINumber { get; set; } = string.Empty;

        [JsonPropertyName("lastModified")]
        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("createdOn")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("createdBy")]
        public User? CreatedBy { get; set; }

        [JsonPropertyName("lastModifiedBy")]
        public User? LastModifiedBy { get; set; }

        [JsonPropertyName("hasChildren")]
        public bool HasChildren { get; set; }
    }
}
