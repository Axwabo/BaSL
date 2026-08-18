using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace BaSL.SourceGenerators;

[Generator]
public sealed class ExecuteGenerator : IIncrementalGenerator
{

    private const string TokenType = "System.Threading.CancellationToken";
    private const string TokenParam = "cancellationToken";

    private static void Execute(MethodToGenerate? method, SourceProductionContext context)
    {
        if (method is not null)
            context.AddSource($"{method.Namespace}.{method.ClassName}.g.cs", SourceText.From(GenerateClass(method), Encoding.UTF8));
    }

    private static string GenerateClass(MethodToGenerate method)
    {
        var sb = new StringBuilder($$"""
                                     #nullable enable
                                     namespace {{method.Namespace}}
                                     {
                                         partial class {{method.ClassName}}
                                         {
                                             public override async global::System.Threading.Tasks.Task<int> ExecuteAsync(global::{{TokenType}} {{TokenParam}})
                                             {
                                     """);
        DeclareOptions(method, sb);
        DetectOptions(method, sb);
        RequireOptions(method, sb);
        sb.Append("            return await ").Append(method.MethodName).Append('(');
        PassOptions(method, sb);
        return sb.Append(");\n        }\n    }\n}").ToString();
    }

    private static void DeclareOptions(MethodToGenerate method, StringBuilder sb)
    {
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
    }

    private static void DetectOptions(MethodToGenerate method, StringBuilder sb)
    {
        // TODO: this sucks
        sb.AppendLine("for (int i = 0; i < this.Args.Length; i++)")
            .AppendLine("{")
            .AppendLine("string arg = this.Args[i];")
            .AppendLine("if (arg.StartsWith('-'))")
            .AppendLine("{")
            .AppendLine("for (int c = 1; c < arg.Length; c++)")
            .AppendLine("{");
        foreach (var option in method.Options)
            if (option is FlagOption flag)
                sb.Append("if (c == '")
                    .Append(flag.Flag)
                    .Append("')")
                    .AppendLine()
                    .AppendLine("{")
                    .Append(flag.Name)
                    .AppendLine(" = true;")
                    .AppendLine("}");
        sb.AppendLine("}").AppendLine("}").AppendLine("}");
    }

    private static void RequireOptions(MethodToGenerate method, StringBuilder sb)
    {
        foreach (var option in method.Options)
            if (option is FlagOption {Required: true, DefaultValue: null, Name: var name})
                sb.Append("if (!")
                    .Append(name)
                    .AppendLine(".HasValue)")
                    .AppendLine("{")
                    .Append("await this.StandardError.WriteLineAsync(\"Argument \\\"")
                    .Append(name)
                    .AppendLine("\\\" must be specified\");")
                    .AppendLine("return 1;")
                    .AppendLine("}");
    }

    private static void PassOptions(MethodToGenerate method, StringBuilder sb)
    {
        foreach (var option in method.Options)
        {
            sb.Append(option is CancellationTokenOption ? "cancellationToken" : option.Name);
            if (option is FlagOption {Required: true})
                sb.Append(".Value");
            sb.Append(", ");
        }

        if (method.Options.Length != 0)
            sb.Remove(sb.Length - 2, 2);
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
                {
                    if (symbol.Type.ToString() == TokenType)
                    {
                        list.Add(new CancellationTokenOption(symbol.Name));
                        continue;
                    }

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
                }

                return new MethodToGenerate(ns, className, methodName, new EquatableArray<Option>(list.ToArray()));
            });

        context.RegisterSourceOutput(provider, (ctx, generate) => Execute(generate, ctx));
    }

}
