
using Stellaris.Shared.Schemas;

namespace Stellaris.WebApi.Payloads
{    
    public record NewOrEditCustomRolePayload(
        string Name, 
        List<PermissionGrant> RepositoryPermissionGrants,
        List<PermissionGrant> BuildPermissionGrants,
        List<PermissionGrant> ReleasePermissionGrants,
        RoleDefinition ServiceEndpointRole,
        RoleDefinition EnvironmentRole,
        RoleDefinition LibraryRole);
    

    public record ModifyFolderPayload(
        string Name, string Kind, string Description,
        string CINumber, User LastModifiedBy);
    public record NewFolderPayload(
        string Name, string Kind, string Description,
        string CINumber, User CreatedBy);
    public record NewLinkPayload(
        string FolderId, string ResourceId, string Kind, string ResourceName, User By);

    public record PermissionDescriptorPayload(string FolderId, string ResourceId, string Kind, List<PermissionDescriptor> PermissionDescriptors);
    public record RoleDescriptorPayload(string FolderId, string ResourceId, string Kind, List<RoleDescriptor> RoleDescriptors);

    public record NewCustomRoleAssignmentPayload(string FolderId, Identity Identity, string CustomRoleId);

    public record CustomRoleAssignmentPayload(
        string RoleAssignmentId, string ProjectId, string FolderId, 
        Identity Identity, string CustomRoleId, string RoleName);

    public record EnforceSecurityPayload(string ResourceId, string ResourceKind);
}