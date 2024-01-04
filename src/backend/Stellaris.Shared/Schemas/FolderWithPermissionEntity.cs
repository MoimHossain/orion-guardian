namespace Stellaris.Shared.Schemas
{
    public class FolderWithPermissionEntity : FolderEntity
    {
        public FolderSecurityEvaluationResult Security { get; set; } = new FolderSecurityEvaluationResult();

        public static FolderWithPermissionEntity From(
            FolderEntity folder, FolderSecurityEvaluationResult security)
        {
            var result = new FolderWithPermissionEntity();

            result.ProjectId = folder.ProjectId;
            result.Id = folder.Id;
            result.Name = folder.Name;
            result.Kind = folder.Kind;
            result.ParentFolderId = folder.ParentFolderId;
            result.Path = folder.Path;
            result.Description = folder.Description;
            result.CINumber = folder.CINumber;
            result.LastModified = folder.LastModified;
            result.CreatedOn = folder.CreatedOn;
            result.CreatedBy = folder.CreatedBy;
            result.LastModifiedBy = folder.LastModifiedBy;
            result.HasChildren = folder.HasChildren;

            result.Security = security;

            return result;
        }
    }
}
