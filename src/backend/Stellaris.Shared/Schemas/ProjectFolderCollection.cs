
using System.Text.Json.Serialization;

namespace Stellaris.Shared.Schemas
{
    public class ProjectFolderCollectionEntity
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("projectId")]
        public string ProjectId { get; set; } = string.Empty;

        [JsonPropertyName("folders")]
        public List<FolderEntity> Folders { get; set; } = new List<FolderEntity>();
    }
}
