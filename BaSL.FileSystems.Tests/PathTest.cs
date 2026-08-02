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

    [Fact]
    public void ToPartialAbsoluteWhenSelfIsRelative()
    {
        var left = new Path("among");
        var us = new Path("/us/sus");
        var absolute = left.ToPartialAbsolute(us);
        Assert.Equal("/us/sus/among", absolute.Value);
    }

    [Fact]
    public void ToPartialAbsoluteWhenSelfIsAbsolute()
    {
        var left = new Path("/among");
        var us = new Path("/us/sus");
        var absolute = left.ToPartialAbsolute(us);
        Assert.Equal("/among", absolute.Value);
    }

}
