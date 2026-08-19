using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BaSL.SourceGenerators;

public static class Helpers
{

    public const string TokenParam = "cancellationToken";

    public static (string Namespace, string Class)? GetParent(SyntaxNode node)
    {
        var type = node as BaseTypeDeclarationSyntax ?? node.Parent as BaseTypeDeclarationSyntax;
        if (type == null)
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

        return (ns, type.Identifier.Text);
    }

    extension(StringBuilder sb)
    {

        public StringBuilder WriteLineAsync(string writer, string text)
            => sb.Append("await BaSL.Executables.StreamWriterExtensions.WriteLineAsync(")
                .Append(writer)
                .Append(", \"")
                .Append(text)
                .AppendLine($"\", {TokenParam});");

    }

}
