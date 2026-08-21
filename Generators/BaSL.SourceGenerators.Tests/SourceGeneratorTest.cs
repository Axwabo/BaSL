using System.IO;
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
        var appLocation = typeof(App).Assembly.Location;
        var netstandardLocation = Path.Combine(Directory.GetParent(appLocation)!.FullName, "netstandard.dll");
        var compilation = CSharpCompilation.Create(
            nameof(SourceGeneratorTest),
            [CSharpSyntaxTree.ParseText(Constants.Source, cancellationToken: CancellationToken.None)],
            [
                MetadataReference.CreateFromFile(appLocation),
                MetadataReference.CreateFromFile(netstandardLocation)
            ]
        );
        var result = driver.RunGenerators(compilation, CancellationToken.None).GetRunResult();
        var syntax = result.GeneratedTrees.Single(e => e.FilePath.EndsWith(".g.cs"));
        Assert.Equal(Constants.Result, syntax.GetText(CancellationToken.None).ToString());
    }

}
