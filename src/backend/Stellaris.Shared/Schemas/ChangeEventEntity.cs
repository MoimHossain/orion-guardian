

using System.Text.Json.Serialization;

namespace Stellaris.Shared.Schemas
{
    public class ChangeEventEntity
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("projectId")]
        public string ProjectId { get; set; } = string.Empty;

        [JsonPropertyName("eventType")]
        public string EventType { get; set; } = string.Empty;

        [JsonPropertyName("folderId")]
        public string FolderId { get; set; } = string.Empty;

        [JsonPropertyName("resourceId")]
        public string ResourceId { get; set; } = string.Empty;

        [JsonPropertyName("customRoleId")]
        public string CustomRoleId { get; set; } = string.Empty;

        [JsonPropertyName("customRoleAssignmentId")]
        public string CustomRoleAssignmentId { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{EventType}:{ProjectId}";
        }

        public static ChangeEventEntity CreateCustomRoleCreatedEventFrom(CustomRoleDefinitionEntity customRoleEntity)
        {
            return new ChangeEventEntity
            {
                ProjectId = customRoleEntity.ProjectId,
                EventType = ChangeEventType.CustomRoleCreated.ToString(),                
                CustomRoleId = customRoleEntity.Id
            };
        }

        public static ChangeEventEntity CreateCustomRoleDeletedEventFrom(string projectId, string roleId)
        {
            return new ChangeEventEntity
            {
                ProjectId = projectId,
                EventType = ChangeEventType.CustomRoleDeleted.ToString(),
                CustomRoleId = roleId
            };
        }

        public static ChangeEventEntity CreateCustomRoleUpdatedEventFrom(CustomRoleDefinitionEntity customRoleEntity)
        {
            return new ChangeEventEntity
            {
                ProjectId = customRoleEntity.ProjectId,
                EventType = ChangeEventType.CustomRoleUpdated.ToString(),
                CustomRoleId = customRoleEntity.Id
            };
        }

        public static ChangeEventEntity CreateRoleAssignmentCreatedEventFrom(CustomRoleAssignmentEntity roleAssignmentEntity)
        {
            return new ChangeEventEntity
            {
                ProjectId = roleAssignmentEntity.ProjectId,
                EventType = ChangeEventType.CustomRoleAssignmentCreated.ToString(),
                CustomRoleAssignmentId = roleAssignmentEntity.Id,
                FolderId = roleAssignmentEntity.FolderId
            };
        }

        public static ChangeEventEntity CreateRoleAssignmentDeletedEventFrom(CustomRoleAssignmentEntity deletedItem)
        {
            return new ChangeEventEntity
            {
                ProjectId = deletedItem.ProjectId,
                EventType = ChangeEventType.CustomRoleAssignmentDeleted.ToString(),
                CustomRoleAssignmentId = deletedItem.Id,
                FolderId = deletedItem.FolderId
            };
        }

        public static ChangeEventEntity CreateLinkCreatedEventFrom(LinkEntity link)
        {
            return new ChangeEventEntity
            {
                ProjectId = link.ProjectId,
                EventType = ChangeEventType.ResourceLinkCreated.ToString(),
                FolderId = link.FolderId,
                ResourceId = link.ResourceId
            };
        }
    }
}
