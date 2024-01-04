

using System.Text.Json.Serialization;


namespace Stellaris.Shared.AzureDevOps.DTOs
{
    public record LocationServiceData(
        [property: JsonPropertyName("serviceOwner")] string ServiceOwner,
        [property: JsonPropertyName("defaultAccessMappingMoniker")] string DefaultAccessMappingMoniker,
        [property: JsonPropertyName("lastChangeId")] long LastChangeId,
        [property: JsonPropertyName("lastChangeId64")] long LastChangeId64
    );

    public record AzureDevOpsUserContext(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("descriptor")] string Descriptor,
        [property: JsonPropertyName("subjectDescriptor")] string SubjectDescriptor,
        [property: JsonPropertyName("providerDisplayName")] string ProviderDisplayName,
        [property: JsonPropertyName("isActive")] bool IsActive
    );

    public record ConnectionDataPayload(
        [property: JsonPropertyName("authenticatedUser")] AzureDevOpsUserContext AuthenticatedUser,
        [property: JsonPropertyName("authorizedUser")] AzureDevOpsUserContext AuthorizedUser,
        [property: JsonPropertyName("instanceId")] string InstanceId,
        [property: JsonPropertyName("deploymentId")] string DeploymentId,
        [property: JsonPropertyName("deploymentType")] string DeploymentType,
        [property: JsonPropertyName("locationServiceData")] LocationServiceData LocationServiceData
    );

    public record AzDoProject(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("url")] string Url,
        [property: JsonPropertyName("state")] string State,
        [property: JsonPropertyName("revision")] int Revision,
        [property: JsonPropertyName("visibility")] string Visibility,
        [property: JsonPropertyName("lastUpdateTime")] DateTime LastUpdateTime
    );

    public class AzDoGenericCollection<T>
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("value")]
        public List<T> Value { get; set; } = new List<T>();
    }
}
