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
        
    }

}
