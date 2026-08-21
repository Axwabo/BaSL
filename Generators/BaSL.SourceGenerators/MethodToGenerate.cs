namespace BaSL.SourceGenerators;

public sealed record MethodToGenerate(string Namespace, string ClassName, string MethodName, EquatableArray<Option> Options);
