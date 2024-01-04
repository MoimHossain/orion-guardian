
using System.Text.Json.Serialization;

namespace Stellaris.Shared.AzureDevOps.DTOs
{
    public record AzdoIdentitySlim(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("descriptor")] string Descriptor,
        [property: JsonPropertyName("subjectDescriptor")] string SubjectDescriptor,
        [property: JsonPropertyName("providerDisplayName")] string ProviderDisplayName,
        [property: JsonPropertyName("customDisplayName")] string CustomDisplayName,
        [property: JsonPropertyName("isActive")] bool IsActive
    );

    public record IdentitySearchPayloadOptions(
        [property: JsonPropertyName("MinResults")] int MinResults,
        [property: JsonPropertyName("MaxResults")] int MaxResults
    );

    public record IdentitySearchPayload(
        [property: JsonPropertyName("query")] string Query,
        [property: JsonPropertyName("identityTypes")] IReadOnlyList<string> IdentityTypes,
        [property: JsonPropertyName("operationScopes")] IReadOnlyList<string> OperationScopes,
        [property: JsonPropertyName("options")] IdentitySearchPayloadOptions Options,
        [property: JsonPropertyName("properties")] IReadOnlyList<string> Properties
    );
    public record TranslateDescriptorPayload(
        [property: JsonPropertyName("subjectDescriptors")] string SubjectDescriptors
    );
    public record AzDoIdentity(
        [property: JsonPropertyName("entityId")] string? EntityId,
        [property: JsonPropertyName("entityType")] string? EntityType,
        [property: JsonPropertyName("originDirectory")] string? OriginDirectory,
        [property: JsonPropertyName("originId")] string? OriginId,
        [property: JsonPropertyName("localDirectory")] string? LocalDirectory,
        [property: JsonPropertyName("localId")] string? LocalId,
        [property: JsonPropertyName("displayName")] string? DisplayName,
        [property: JsonPropertyName("scopeName")] string? ScopeName,
        [property: JsonPropertyName("samAccountName")] string? SamAccountName,
        [property: JsonPropertyName("subjectDescriptor")] string? SubjectDescriptor,
        [property: JsonPropertyName("department")] string? Department,
        [property: JsonPropertyName("jobTitle")] string? JobTitle,
        [property: JsonPropertyName("mail")] string? Mail,
        [property: JsonPropertyName("mailNickname")] string? MailNickname,
        [property: JsonPropertyName("physicalDeliveryOfficeName")] string? PhysicalDeliveryOfficeName,
        [property: JsonPropertyName("signInAddress")] string? SignInAddress,
        [property: JsonPropertyName("surname")] string? Surname,
        [property: JsonPropertyName("guest")] bool? Guest,
        [property: JsonPropertyName("description")] string? Description,
        [property: JsonPropertyName("isMru")] bool? IsMru
    );

    public record AzDoIdentityCollection(
        [property: JsonPropertyName("queryToken")] string QueryToken,
        [property: JsonPropertyName("identities")] IReadOnlyList<AzDoIdentity> Identities,
        [property: JsonPropertyName("pagingToken")] string PagingToken
    );

    public record AzDoSearchResponse(
        [property: JsonPropertyName("results")] IReadOnlyList<AzDoIdentityCollection> Results
    );

    public record AzDoGroupMaterializeResponse(
        [property: JsonPropertyName("subjectKind")] string SubjectKind,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("isCrossProject")] bool IsCrossProject,
        [property: JsonPropertyName("domain")] string Domain,
        [property: JsonPropertyName("principalName")] string PrincipalName,
        [property: JsonPropertyName("mailAddress")] object MailAddress,
        [property: JsonPropertyName("origin")] string Origin,
        [property: JsonPropertyName("originId")] string OriginId,
        [property: JsonPropertyName("displayName")] string DisplayName,
        [property: JsonPropertyName("url")] string Url,
        [property: JsonPropertyName("descriptor")] string Descriptor
    );

    public record AzDoTranslatedIdentityDescriptor(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("descriptor")] string Descriptor,
        [property: JsonPropertyName("subjectDescriptor")] string SubjectDescriptor,
        [property: JsonPropertyName("providerDisplayName")] string ProviderDisplayName
    );

    public record AzDoDescriptorTranslationResponse(
        [property: JsonPropertyName("count")] int Count,
        [property: JsonPropertyName("value")] IReadOnlyList<AzDoTranslatedIdentityDescriptor> Value
    );

    public record AzDoMembershipResponse(
        [property: JsonPropertyName("containerDescriptor")] string ContainerDescriptor,
        [property: JsonPropertyName("memberDescriptor")] string MemberDescriptor
    );

    public static class BuiltInAccountType
    {
        public const string ProjectCollectionAdministrators = "Project Collection Administrators";
        public const string ProjectAdministrators = "Project Administrators";
        public const string ProjectCollectionBuildService = "Project Collection Build Service";
        public const string ProjectBuildService = "Build Service";
    }
}
