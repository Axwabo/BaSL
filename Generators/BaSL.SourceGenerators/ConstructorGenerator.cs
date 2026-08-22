using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace BaSL.SourceGenerators;

[Generator]
public sealed class ConstructorGenerator : IIncrementalGenerator
{

    private const string ContextType = "BaSL.Executables.ExecutableContext";

    private static void Execute(ConstructorToGenerate? ctor, SourceProductionContext context)
    {
        if (ctor is not null)
            context.AddSource($"{ctor.Namespace}.{ctor.ClassName}.Constructor.g.cs", SourceText.From(GenerateClass(ctor), Encoding.UTF8));
    }

    private static string GenerateClass(ConstructorToGenerate help)
        => $$"""
             namespace {{help.Namespace}}
             {
                 partial class {{help.ClassName}}
                 {
                     public {{help.ClassName}}({{ContextType}} context) : base(context)
                     {
                     }
                 }
             }   
             """;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.CreateSyntaxProvider(
            (node, _) => node is ClassDeclarationSyntax {BaseList.Types.Count: not 0},
            (ctx, token)
                => ctx.SemanticModel.GetDeclaredSymbol(ctx.Node, token) is INamedTypeSymbol {BaseType: { } baseType, InstanceConstructors: var constructors}
                   && (constructors.Length == 0 || constructors[0].IsImplicitlyDeclared)
                   && baseType.ToString() == "BaSL.Executables.App"
                   && Helpers.GetParent(ctx.Node) is var (ns, name)
                    ? new ConstructorToGenerate(ns, name)
                    : null);

        context.RegisterSourceOutput(provider, (ctx, generate) => Execute(generate, ctx));
    }

}
