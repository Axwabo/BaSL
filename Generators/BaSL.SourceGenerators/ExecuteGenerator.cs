using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace BaSL.SourceGenerators;

[Generator]
public sealed class ExecuteGenerator : IIncrementalGenerator
{

    private static void Execute(MethodToGenerate? method, SourceProductionContext context)
    {
        if (method is not null)
            context.AddSource($"{method.Namespace}.{method.ClassName}.g.cs", SourceText.From(GenerateClass(method), Encoding.UTF8));
    }

    private static string GenerateClass(MethodToGenerate method)
    {
        var sb = new StringBuilder($$"""
                                     namespace {{method.Namespace}}
                                     {
                                         partial class {{method.ClassName}}
                                         {
                                             public override async global::System.Threading.Tasks.Task<int> ExecuteAsync(global::System.Threading.CancellationToken cancellationToken)
                                             {
                                     """);
        foreach (var option in method.Options)
        {
            if (option is not FlagOption flag)
                continue;
            sb.Append("bool? ").Append(option.Name).Append(" = ").Append(flag.DefaultValue switch
            {
                true => "true",
                false => "false",
                null => "null"
            }).AppendLine(";");
        }

        sb.Append("            return await ").Append(method.MethodName).Append('(');
        foreach (var option in method.Options)
        {
            sb.Append(option.Name);
            if (option is FlagOption {Required: true})
                sb.Append(".Value");
            sb.Append(", ");
        }

        if (method.Options.Length != 0)
            sb.Remove(sb.Length - 2, 2);
        return sb.Append(");\n        }\n    }\n}").ToString();
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "BaSL.Executables.Attributes.ExecuteAttribute",
            (_, _) => true,
            (ctx, token) =>
            {
                if (ctx.TargetSymbol is not IMethodSymbol {Name: var methodName, Parameters: var parameters}
                    || Helpers.GetParent(ctx.TargetNode) is not var (ns, className))
                    return null;
                var list = new List<Option>();
                foreach (var symbol in parameters)
                foreach (var attribute in symbol.GetAttributes())
                {
                    token.ThrowIfCancellationRequested();
                    if (attribute.AttributeClass?.ToString() == "BaSL.Executables.Attributes.FlagAttribute")
                        list.Add(new FlagOption(
                            symbol.Name,
                            (char) attribute.ConstructorArguments[0].Value!,
                            symbol.NullableAnnotation != NullableAnnotation.Annotated,
                            symbol.HasExplicitDefaultValue ? symbol.ExplicitDefaultValue as bool? : null
                        )); // TODO: other options
                }

                return new MethodToGenerate(ns, className, methodName, new EquatableArray<Option>(list.ToArray()));
            });

        context.RegisterSourceOutput(provider, (ctx, generate) => Execute(generate, ctx));
    }

}
