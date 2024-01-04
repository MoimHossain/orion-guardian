

using System.Text.Json.Serialization;

namespace Stellaris.Shared.Schemas
{
    public class Privilege : ICanValidate
    {
        [JsonPropertyName("bit")]
        public int Bit { get; set; } = 0;

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; } = string.Empty;

        public bool IsValid()
        {
            if (Bit == 0)
                return false;
            return true;
        }
    }
}
