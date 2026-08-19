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
    private const string RestType = "BaSL.Args";
    private const string IndexVar = "positionalArgumentIndex";
    private const string RestVar = "restIndex";

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
            switch (option)
            {
                case FlagOption flag:
                    sb.Append("bool? ").Append(option.Name).Append(" = ").Append(flag.DefaultValue switch
                    {
                        true => "true",
                        false => "false",
                        null => "null"
                    }).AppendLine(";");
                    break;
                case PositionalOption positional:
                    // TODO: optional
                    sb.Append(positional.Type).Append(' ').Append(option.Name).Append(" = ").Append(positional.DefaultValue ?? "null").AppendLine(";");
                    break;
            }
        }
    }

    private static void DetectOptions(MethodToGenerate method, StringBuilder sb)
    {
        // TODO: this sucks
        // TODO: arguments with values
        var positionalArgumentIndex = 0;
        sb.AppendLine($"int {IndexVar} = 0;")
            .AppendLine($"int {RestVar} = 0;")
            .AppendLine("for (int i = 0; i < this.Args.Length; i++)")
            .AppendLine("{")
            .AppendLine("string arg = this.Args[i];")
            .AppendLine("if (arg == \"--\")")
            .AppendLine("{")
            .AppendLine($"{RestVar} = i + 1;")
            .AppendLine("break;")
            .AppendLine("}")
            .AppendLine("if (arg.StartsWith(\"-\"))")
            .AppendLine("{")
            .AppendLine($"{RestVar} = i + 1;")
            .AppendLine("for (int c = 1; c < arg.Length; c++)")
            .AppendLine("{");
        foreach (var option in method.Options)
            if (option is FlagOption flag)
                sb.Append("if (arg[c] == '")
                    .Append(flag.Flag)
                    .Append("')")
                    .AppendLine()
                    .AppendLine("{")
                    .Append(flag.Name)
                    .AppendLine(" = true;")
                    .AppendLine("}");
        sb.AppendLine("}").AppendLine("continue;").AppendLine("}");
        string? rest = null;
        foreach (var option in method.Options)
        {
            if (option is RestArgumentsOption)
            {
                rest = option.Name;
                continue;
            }

            if (option is not PositionalOption {Type: var type, Name: var name})
                continue;
            var i = positionalArgumentIndex++;
            sb.Append($"if ({IndexVar} == ").Append(i).AppendLine(")").AppendLine("{");
            if (type is "string" or "string?")
                sb.Append(option.Name).AppendLine(" = arg;");
            else
                sb.Append("if (!BaSL.Executables.ArgumentParser<")
                    .Append(type)
                    .Append(">.TryParse(arg, out var tryParse_")
                    .Append(name)
                    .AppendLine(")")
                    .AppendLine("{")
                    .Append("await this.StandardError.WriteLineAsync(\"Invalid value for argument \\\"")
                    .Append(name)
                    .AppendLine("\\\");")
                    .AppendLine("return 1;")
                    .AppendLine("}")
                    .Append(name)
                    .Append(" = tryParse_")
                    .Append(name)
                    .AppendLine(";")
                    .AppendLine($"{RestVar} = {IndexVar} + 2;");
            sb.AppendLine("}");
        }

        sb.AppendLine($"{IndexVar}++;").AppendLine("}");
        if (rest != null)
            sb.Append(RestType).Append(' ').Append(rest).Append(" = ").Append($"this.Args.Length <= {RestVar} ? default : this.Args[{RestVar}..];");
    }

    private static void RequireOptions(MethodToGenerate method, StringBuilder sb)
    {
        // TODO: positional options
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
                    var type = symbol.Type.ToString();
                    if (type == TokenType)
                    {
                        list.Add(new CancellationTokenOption(symbol.Name));
                        continue;
                    }

                    if (type == RestType)
                    {
                        list.Add(new RestArgumentsOption(symbol.Name));
                        continue;
                    }

                    var isFlag = false;
                    foreach (var attribute in symbol.GetAttributes())
                    {
                        token.ThrowIfCancellationRequested();
                        if (attribute.AttributeClass?.ToString() != "BaSL.Executables.Attributes.FlagAttribute")
                            continue;
                        isFlag = true;
                        list.Add(new FlagOption(
                            symbol.Name,
                            attribute.ConstructorArguments.Length != 0 && attribute.ConstructorArguments[0].Value is char flagChar ? flagChar : symbol.Name[0],
                            symbol.NullableAnnotation != NullableAnnotation.Annotated,
                            symbol.HasExplicitDefaultValue ? symbol.ExplicitDefaultValue as bool? : null
                        )); // TODO: other options
                    }

                    if (!isFlag)
                        list.Add(new PositionalOption(symbol.Name, type, symbol.HasExplicitDefaultValue ? symbol.ExplicitDefaultValue?.ToString() : null));
                }

                return new MethodToGenerate(ns, className, methodName, new EquatableArray<Option>(list.ToArray()));
            });

        context.RegisterSourceOutput(provider, (ctx, generate) => Execute(generate, ctx));
    }

}
