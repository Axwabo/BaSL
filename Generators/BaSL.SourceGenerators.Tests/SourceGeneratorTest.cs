extern alias netstandard;
using System.Linq;
using System.Threading;
using BaSL.Executables;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace BaSL.SourceGenerators.Tests;

public sealed class SourceGeneratorTest
{

    [Fact]
    public void Flag()
    {
        var generator = new ExecuteGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        var compilation = CSharpCompilation.Create(
            nameof(SourceGeneratorTest),
            [CSharpSyntaxTree.ParseText(Constants.Source, cancellationToken: CancellationToken.None)],
            [
                // MetadataReference.CreateFromFile(typeof(netstandard::System.Attribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(App).Assembly.Location),
            ]
        );
        var result = driver.RunGenerators(compilation, CancellationToken.None).GetRunResult();
        var syntax = result.GeneratedTrees.Single(e => e.FilePath.EndsWith(".g.cs"));
        Assert.Equal(Constants.Result, syntax.GetText(CancellationToken.None).ToString());
    }

}
