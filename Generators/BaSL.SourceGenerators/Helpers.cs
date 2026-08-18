using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BaSL.SourceGenerators;

public static class Helpers
{

    public static (string Namespace, string Class)? GetParent(SyntaxNode node)
    {
        if (node.Parent is not BaseTypeDeclarationSyntax type)
            return null;
        var parent = type.Parent;
        while (parent is not (null or NamespaceDeclarationSyntax or FileScopedNamespaceDeclarationSyntax))
            parent = parent.Parent;
        if (parent is not BaseNamespaceDeclarationSyntax syntax)
            return ("", type.Identifier.Span.ToString());
        var ns = syntax.Name.ToString();
        while (syntax.Parent is NamespaceDeclarationSyntax outer)
        {
            ns = $"{outer.Name}.{ns}";
            syntax = outer;
        }

        return (ns, type.Identifier.Span.ToString());
    }

}
