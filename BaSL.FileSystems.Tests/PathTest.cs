namespace BaSL.FileSystems.Tests;

public sealed class PathTest
{

    [Fact]
    public void CombineTwoSimple()
    {
        var left = new Path("among");
        var right = new Path("us");
        var combined = left / right;
        Assert.Equal("among/us", combined.Value);
    }

    [Fact]
    public void CombineLeftTrailingSlash()
    {
        var left = new Path("among/");
        var right = new Path("us");
        var combined = left / right;
        Assert.Equal("among/us", combined.Value);
    }

    [Fact]
    public void CombineRightLeadingSlash()
    {
        var left = new Path("among/");
        var right = new Path("us");
        var combined = left / right;
        Assert.Equal("among/us", combined.Value);
    }

}
