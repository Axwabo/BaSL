using Microsoft.CodeAnalysis;

namespace BaSL.SourceGenerators;

public sealed class ExecuteGenerator : IIncrementalGenerator
{

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "BaSL.Executables.ExecuteAttribute",
            (_, _) => true,
            (ctx, _) => ctx.SemanticModel.GetDeclaredSymbol(ctx.TargetNode) is IMethodSymbol
        );
    }

}
