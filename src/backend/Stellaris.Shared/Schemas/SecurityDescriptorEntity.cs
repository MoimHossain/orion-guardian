using System.Text.Json.Serialization;

namespace Stellaris.Shared.Schemas
{
    public class SecurityDescriptorEntity
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("projectId")]
        public string ProjectId { get; set; } = string.Empty;

        [JsonPropertyName("folderId")]
        public string FolderId { get; set; } = string.Empty;

        [JsonPropertyName("kind")]
        public string Kind { get; set; } = string.Empty;

        [JsonPropertyName("permissionDescriptors")]
        public List<PermissionDescriptor> PermissionDescriptors { get; set; } = new List<PermissionDescriptor>();

        [JsonPropertyName("roleDescriptors")]
        public List<RoleDescriptor> RoleDescriptors { get; set; } = new List<RoleDescriptor>();
    }
}
