using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace BaSL.SourceGenerators;

public sealed class ExecuteGenerator : IIncrementalGenerator
{

    private static void Execute(MethodToGenerate? method, SourceProductionContext context)
    {
        if (method is not null)
            context.AddSource($"{method.Namespace}.{method.ClassName}.g.cs", SourceText.From(GenerateClass(method)));
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
            });
        }

        sb.Append("            return ExecuteAsync(");
        foreach (var option in method.Options)
            sb.Append(option.Name).Append(", ");
        if (method.Options.Length != 0)
            sb.Remove(sb.Length - 3, 2);
        return sb.Append(");\n        }\n    }\n}").ToString();
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "BaSL.Executables.Attributes.ExecuteAttribute",
            (_, _) => true,
            (ctx, token) =>
            {
                if (ctx.TargetSymbol is not IMethodSymbol {Parameters: var parameters}
                    || Helpers.GetParent(ctx.TargetNode) is not var (ns, name))
                    return null;
                var list = new List<Option>();
                foreach (var symbol in parameters)
                foreach (var attribute in symbol.GetAttributes())
                {
                    token.ThrowIfCancellationRequested();
                    if (attribute.AttributeClass is {Name: "FlagAttribute", ContainingNamespace.Name: "BaSL.Executables.Attributes"})
                        list.Add(new FlagOption(symbol.Name, (char) attribute.ConstructorArguments[0].Value!, symbol.NullableAnnotation != NullableAnnotation.Annotated, symbol.ExplicitDefaultValue as bool?)); // TODO: other options
                }

                return new MethodToGenerate(ns, name, new EquatableArray<Option>(list.ToArray()));
            });

        context.RegisterSourceOutput(provider, (ctx, generate) => Execute(generate, ctx));
    }

}
