using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace BaSL.SourceGenerators;

[Generator]
public sealed class HelpGenerator : IIncrementalGenerator
{

    private static void Execute(HelpToGenerate? help, SourceProductionContext context)
    {
        if (help is not null)
            context.AddSource($"{help.Namespace}.{help.ClassName}.Help.g.cs", SourceText.From(GenerateClass(help), Encoding.UTF8));
    }

    private static string GenerateClass(HelpToGenerate help)
    {
        var literal = SymbolDisplay.FormatLiteral(help.HelpText, true);
        return $$"""
                 namespace {{help.Namespace}}
                 {
                     partial class {{help.ClassName}} : BaSL.Executables.IHelpProvider
                     {
                         public async System.Threading.Tasks.Task DisplayHelpAsync({{Helpers.TokenType}} {{Helpers.TokenParam}})
                         {
                             await BaSL.Executables.StreamWriterExtensions.WriteLineAsync(this.StandardOutput, {{literal}}, {{Helpers.TokenParam}}); 
                         }
                     }
                 }   
                 """;
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "BaSL.Executables.Attributes.HelpAttribute", (node, _) => node is ClassDeclarationSyntax,
            (ctx, _) =>
            {
                foreach (var attribute in ctx.Attributes)
                {
                    if (attribute.AttributeClass?.ToString() != "BaSL.Executables.Attributes.HelpAttribute"
                        || attribute.ConstructorArguments.Length != 1
                        || attribute.ConstructorArguments[0].Value is not string help
                        || Helpers.GetParent(ctx.TargetNode) is not var (ns, name))
                        continue;
                    return new HelpToGenerate(ns, name, help);
                }

                return null;
            }
        );

        context.RegisterSourceOutput(provider, (ctx, generate) => Execute(generate, ctx));
    }

}
