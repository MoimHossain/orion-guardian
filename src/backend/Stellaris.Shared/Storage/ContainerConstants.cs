
namespace Stellaris.Shared.Storage
{
    public class ContainerConstants
    {
        public const string CONTAINERID_FOLDERS = "folders";
        public const string CONTAINERID_LINKS = "resourceLinks";
        public const string CONTAINERID_SECURITY = "security";
        public const string CONTAINERID_AUDIT = "audits";
        public const string CONTAINERID_CUSTOMROLES = "customRoles";
        public const string CONTAINERID_CUSTOMROLES_ASSIGNMENTS = "customRolesAssignments";
        public const string CONTAINERID_SECURITY_ENFORCEMENT = "securityEnforcement";
        public const string CONTAINERID_CHANGE_EVENTS = "changeEvents";
        public const string CONTAINERID_FEED_PROCESSOR_LEASES = "feedProcessingLeases";

        public static readonly List<(string, string)> CONTAINERS =
        [
            (CONTAINERID_FOLDERS, "/projectId"),
            (CONTAINERID_LINKS, "/projectId"),
            (CONTAINERID_SECURITY, "/projectId"),
            (CONTAINERID_AUDIT, "/projectId"),
            (CONTAINERID_CUSTOMROLES, "/projectId"),
            (CONTAINERID_CUSTOMROLES_ASSIGNMENTS, "/projectId"),
            (CONTAINERID_SECURITY_ENFORCEMENT, "/partitionKey"),
            (CONTAINERID_CHANGE_EVENTS, "/projectId"),
            (CONTAINERID_FEED_PROCESSOR_LEASES, "/id")
        ];
    }
}
