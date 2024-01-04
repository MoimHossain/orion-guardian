

using System.Text.Json.Serialization;

namespace Stellaris.Shared.Schemas
{
    public record BuiltInUserPrivilege(
        [property: JsonPropertyName("isAdmin")] bool IsAdmin,
        [property: JsonPropertyName("isProjectCollectionAdmin")] bool IsProjectCollectionAdmin,
        [property: JsonPropertyName("isProjectAdmin")] bool IsProjectAdmin
    );
}
