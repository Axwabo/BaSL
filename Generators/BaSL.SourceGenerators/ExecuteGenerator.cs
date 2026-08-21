using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace BaSL.SourceGenerators;

[Generator]
public sealed class ExecuteGenerator : IIncrementalGenerator
{

    private const string IndexVar = "positionalArgumentIndex";
    private const string RestVar = "restIndex";
    private const string DirectoryResultPrefix = "resolveDirectory_";
    private const string TryParsePrefix = "tryParse_";

    private static void Execute(MethodToGenerate? method, SourceProductionContext context)
    {
        if (method is not null)
            context.AddSource($"{method.Namespace}.{method.ClassName}.Execute.g.cs", SourceText.From(GenerateClass(method), Encoding.UTF8));
    }

    private static string GenerateClass(MethodToGenerate method)
    {
        var sb = new StringBuilder($$"""
                                     #nullable enable
                                     namespace {{method.Namespace}}
                                     {
                                         partial class {{method.ClassName}}
                                         {
                                             public override async global::System.Threading.Tasks.Task<int> ExecuteAsync(global::{{Helpers.TokenType}} {{Helpers.TokenParam}})
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
                case DirectoryOption:
                    sb.Append($"{Helpers.DirectoryType}? ").Append(option.Name).AppendLine(" = null;");
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

            if (option is not IPositionalOption)
                continue;
            var i = positionalArgumentIndex++;
            sb.Append($"if ({IndexVar} == ").Append(i).AppendLine(")").AppendLine("{");
            switch (option)
            {
                case PositionalOption {Type: var type, Name: var name}:
                    AppendPositionalOption(sb, type, name);
                    break;
                case DirectoryOption:
                    AppendDirectoryOption(sb, option.Name);
                    break;
            }

            sb.AppendLine($"{RestVar} = {IndexVar} + 2;").AppendLine("}");
        }

        sb.AppendLine($"{IndexVar}++;").AppendLine("}");
        if (rest != null)
            sb.Append(Helpers.RestType).Append(' ').Append(rest).Append(" = ").Append($"this.Args.Length <= {RestVar} ? default : this.Args[{RestVar}..];");
    }

    private static void AppendPositionalOption(StringBuilder sb, string type, string name)
    {
        if (type is "string" or "string?")
            sb.Append(name).AppendLine(" = arg;");
        else
            sb.Append("if (!BaSL.Executables.ArgumentParser<")
                .Append(type.EndsWith("?") ? type.Substring(0, type.Length - 1) : type)
                .Append($">.TryParse(arg, out var {TryParsePrefix}")
                .Append(name)
                .AppendLine("))")
                .AppendLine("{")
                .WriteLineAsync("this.StandardError", $"Invalid value for argument '{name}'")
                .AppendLine("return 1;")
                .AppendLine("}")
                .Append(name)
                .Append($" = {TryParsePrefix}")
                .Append(name)
                .AppendLine(";");
    }

    private static void AppendDirectoryOption(StringBuilder sb, string name) => sb.Append($"var {DirectoryResultPrefix}")
        .Append(name)
        .AppendLine(" = BaSL.FileSystems.Extensions.DirectoryExtensions.ResolveDirectory(this.WorkingDirectory, arg);")
        .Append($"if (!{DirectoryResultPrefix}")
        .Append(name)
        .AppendLine(".Success)")
        .AppendLine("{")
        .WriteLineAsyncRaw("this.StandardError", $"{DirectoryResultPrefix}{name}.Error.Message")
        .AppendLine("return 1;")
        .AppendLine("}")
        .Append(name)
        .Append($" = {DirectoryResultPrefix}")
        .Append(name)
        .AppendLine(".Value;");

    private static void RequireOptions(MethodToGenerate method, StringBuilder sb)
    {
        // TODO: positional options
        foreach (var option in method.Options)
            switch (option)
            {
                case FlagOption {Required: true, DefaultValue: null, Name: var name}:
                    sb.Append("if (!")
                        .Append(name)
                        .AppendLine(".HasValue)")
                        .AppendLine("{")
                        .WriteLineAsync("this.StandardError", $"Argument '{name}' must be specified")
                        .AppendLine("return 1;")
                        .AppendLine("}");
                    break;
                case DirectoryOption {DefaultToCurrent: true}:
                    sb.Append(option.Name).AppendLine(" ??= this.WorkingDirectory;");
                    break;
                case DirectoryOption {DefaultToCurrent: false, Name: var name}:
                    sb.Append("if (")
                        .Append(name)
                        .AppendLine(" == null)")
                        .AppendLine("{")
                        .WriteLineAsync("this.StandardError", $"Directory '{name}' must be specified")
                        .AppendLine("return 1;")
                        .AppendLine("}");
                    break;
            }
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
                    token.ThrowIfCancellationRequested();
                    OptionParser.ProcessParameter(symbol, list, token);
                }

                return new MethodToGenerate(ns, className, methodName, new EquatableArray<Option>(list.ToArray()));
            });

        context.RegisterSourceOutput(provider, (ctx, generate) => Execute(generate, ctx));
    }

}
