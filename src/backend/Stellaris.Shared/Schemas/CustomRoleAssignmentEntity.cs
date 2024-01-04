

using System.Text.Json.Serialization;

namespace Stellaris.Shared.Schemas
{
    public class CustomRoleAssignmentEntity
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("projectId")]
        public string ProjectId { get; set; } = string.Empty;

        [JsonPropertyName("folderId")]
        public string FolderId { get; set; } = string.Empty;

        [JsonPropertyName("identity")]
        public Identity? Identity { get; set; }

        [JsonPropertyName("customRoleId")]
        public string CustomRoleId { get; set; } = string.Empty;
    }
}
