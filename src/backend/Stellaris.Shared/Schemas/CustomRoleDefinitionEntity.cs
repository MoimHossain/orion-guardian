
using System.Text.Json.Serialization;

namespace Stellaris.Shared.Schemas
{
    public class CustomRoleDefinitionEntity
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("projectId")]
        public string ProjectId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("repositoryPermissionGrants")]
        public List<PermissionGrant> RepositoryPermissionGrants { get; set; } = new List<PermissionGrant>();

        [JsonPropertyName("buildPermissionGrants")]
        public List<PermissionGrant> BuildPermissionGrants { get; set; } = new List<PermissionGrant>();

        [JsonPropertyName("releasePermissionGrants")]
        public List<PermissionGrant> ReleasePermissionGrants { get; set; } = new List<PermissionGrant>();

        [JsonPropertyName("serviceEndpointRole")]
        public RoleDefinition ServiceEndpointRole { get; set; } = new RoleDefinition();

        [JsonPropertyName("environmentRole")]
        public RoleDefinition EnvironmentRole { get; set; } = new RoleDefinition();

        [JsonPropertyName("libraryRole")]
        public RoleDefinition LibraryRole { get; set; } = new RoleDefinition();
    }
}
