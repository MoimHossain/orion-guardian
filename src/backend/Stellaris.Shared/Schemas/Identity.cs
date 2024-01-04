
using System.Text.Json.Serialization;

namespace Stellaris.Shared.Schemas
{
    public class Identity : ICanValidate
    {
        [JsonPropertyName("entityId")]
        public string EntityId { get; set; } = string.Empty;

        [JsonPropertyName("entityType")]
        public string EntityType { get; set; } = string.Empty;

        [JsonPropertyName("originDirectory")]
        public string OriginDirectory { get; set; } = string.Empty;

        [JsonPropertyName("originId")]
        public string OriginId { get; set; } = string.Empty;

        [JsonPropertyName("localDirectory")]
        public string LocalDirectory { get; set; } = string.Empty;

        [JsonPropertyName("localId")]
        public string LocalId { get; set; } = string.Empty;

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; } = string.Empty;

        [JsonPropertyName("scopeName")]
        public string ScopeName { get; set; } = string.Empty;

        [JsonPropertyName("samAccountName")]
        public string SamAccountName { get; set; } = string.Empty;

        [JsonPropertyName("subjectDescriptor")]
        public string SubjectDescriptor { get; set; } = string.Empty;

        public bool IsValid()
        {
            if(string.IsNullOrWhiteSpace(EntityId))
                return false;
            //if(string.IsNullOrWhiteSpace(EntityType))
            //    return false;
            //if(string.IsNullOrWhiteSpace(OriginDirectory))
            //    return false;
            if(string.IsNullOrWhiteSpace(OriginId))
                return false;
            //if(string.IsNullOrWhiteSpace(LocalDirectory))
            //    return false;
            //if(string.IsNullOrWhiteSpace(LocalId))
            //    return false;
            if(string.IsNullOrWhiteSpace(DisplayName))
                return false;
            //if(string.IsNullOrWhiteSpace(ScopeName))
            //    return false;
            //if(string.IsNullOrWhiteSpace(SamAccountName))
            //    return false;
            //if(string.IsNullOrWhiteSpace(SubjectDescriptor))
            //    return false;
            return true;
        }
    }    
}
