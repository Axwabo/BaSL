namespace BaSL.SourceGenerators;

public sealed record MethodToGenerate(string Namespace, string ClassName, EquatableArray<Option> Options);
