using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BaSL.SourceGenerators;

public sealed class ExecuteGenerator : IIncrementalGenerator
{

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "BaSL.Executables.ExecuteAttribute",
            (_, _) => true,
            (ctx, token) =>
            {
                if (ctx.TargetNode.Parent is not MethodDeclarationSyntax syntax || ctx.SemanticModel.GetDeclaredSymbol(syntax, token) is not INamedTypeSymbol { })
                    return null;
                Unsafe.As<>()
                return ctx.TargetNode.Parent;
            });
    }

}
