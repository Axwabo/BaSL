// ReSharper disable once CheckNamespace

namespace System.Runtime.CompilerServices;

internal sealed class RequiredMemberAttribute : Attribute;

internal sealed class CompilerFeatureRequiredAttribute : Attribute
{

    public const string RefStructs = nameof(RefStructs);

    public const string RequiredMembers = nameof(RequiredMembers);

    public CompilerFeatureRequiredAttribute(string featureName) => FeatureName = featureName;

    public string FeatureName { get; }

    public bool IsOptional { get; init; }

}
