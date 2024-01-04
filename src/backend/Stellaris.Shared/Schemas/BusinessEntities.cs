

namespace Stellaris.Shared.Schemas
{
    public class FolderSecurityEvaluationResult
    {
        public bool IsAdmin { get; set; } = false;
        public bool IsContributor { get; set; } = false;
        public bool IsReader { get; set; } = false;
        public List<FolderSecurityGroupMembershipEvaluationResult> Memberships { get; init; } = [];
    }

    public record FolderSecurityGroupMembershipEvaluationResult(
        string RoleName, string GroupName, string SubjectDescriptor);

   
}
