

using Newtonsoft.Json;
using Stellaris.Shared.Schemas;
using System.Text.Json.Serialization;

namespace Stellaris.Shared.AzureDevOps.DTOs
{
    public class AzDoAclDictionaryEntry
    {
        [JsonPropertyName("descriptor")]
        public string Descriptor { get; set; } = string.Empty;

        [JsonPropertyName("allow")]
        public int Allow { get; set; } = 0;

        [JsonPropertyName("deny")]
        public int Deny { get; set; } = 0;

        public AzDoAclDictionaryEntry(string descriptor, int allow, int deny)
        {
            Descriptor = descriptor;
            Allow = allow;
            Deny = deny;
        }

        public static AzDoAclDictionaryEntry GeneratePermission(string descriptor, int allowBit = 0, int denyBit = 0)
        {
            var allow = 0;
            var deny = 0;
            var aclEntry = new AzDoAclDictionaryEntry(descriptor, (allow | allowBit), (deny | denyBit));
            return aclEntry;
        }
    }


    public record AzDoAclEntryCollection(
        [property: JsonPropertyName("inheritPermissions")] bool InheritPermissions,
        [property: JsonPropertyName("token")] string Token,
        [property: JsonPropertyName("acesDictionary")] Dictionary<string, AzDoAclDictionaryEntry> AcesDictionary
    );

    public record AzDoAclEntryPostBody([property: JsonPropertyName("value")] AzDoAclEntryCollection[] Value);

    public record AzDoRoleAssignment([property: JsonPropertyName("userId")] Guid UserId, [property: JsonPropertyName("roleName")] string RoleName);

    public record AzDoDistributedTaskRoleAssignmentUser(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("uniqueName")] string UniqueName,
        [property: JsonPropertyName("displayName")] string DisplayName);

    public record AzDoDistributedTaskRoleAssignment(
        [property: JsonPropertyName("access")] string Access, 
        [property: JsonPropertyName("accessDisplayName")] string AccessDisplayName,
        [property: JsonPropertyName("identity")] AzDoDistributedTaskRoleAssignmentUser Identity,
        [property: JsonPropertyName("role")] RoleDefinition Role);
    
}
