using BaSL.FileSystems.Extensions;

namespace BaSL.FileSystems.Tests;

public sealed class PathTests
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

    [Fact]
    public void RemoveRelativeSegmentsNone()
    {
        const string path = "among/us";
        var removed = Path.RemoveRelativeSegments(path);
        Assert.Equal(path, removed);
    }

    [Fact]
    public void RemoveRelativeSegmentsNoneAbsolute()
    {
        const string path = "/among/us";
        var removed = Path.RemoveRelativeSegments(path);
        Assert.Equal(path, removed);
    }

    [Fact]
    public void RemoveRelativeSegments()
    {
        var removed = Path.RemoveRelativeSegments("among/sus/../in/./real/life");
        Assert.Equal("among/in/real/life", removed);
    }

    [Fact]
    public void RemoveRelativeSegmentsAbsolute()
    {
        var removed = Path.RemoveRelativeSegments("/among/sus/../in/./real/life");
        Assert.Equal("/among/in/real/life", removed);
    }

    [Fact]
    public void RemoveRelativeSegmentsOutOfBounds()
    {
        var removed = Path.RemoveRelativeSegments("among/..");
        Assert.Equal("", removed);
    }

    [Fact]
    public void RemoveRelativeSegmentsOutOfBoundsAbsolute()
    {
        var removed = Path.RemoveRelativeSegments("/among/..");
        Assert.Equal("/", removed);
    }

    [Fact]
    public void ToAbsoluteWhenAbsolute()
    {
        const string original = "/among/us/in";
        var partial = new Path(original);
        var basePath = new Path("/real/life");
        var absolute = partial.ToAbsolute(basePath);
        Assert.Equal(original, absolute.Value);
    }

    [Fact]
    public void ToAbsolute()
    {
        var partial = new Path("among/us/in");
        var basePath = new Path("/real/life");
        var absolute = partial.ToAbsolute(basePath);
        Assert.Equal("/real/life/among/us/in", absolute.Value);
    }

    [Fact]
    public void ToAbsoluteIncludingRelative()
    {
        var partial = new Path("among/us/../us/in");
        var basePath = new Path("/real/life");
        var absolute = partial.ToAbsolute(basePath);
        Assert.Equal("/real/life/among/us/in", absolute.Value);
    }

    [Fact]
    public void ToAbsoluteCommon()
    {
        var partial = new Path("life/among/us");
        var basePath = new Path("/real/life");
        var absolute = partial.ToAbsolute(basePath);
        Assert.Equal("/real/life/life/among/us", absolute.Value);
    }

}
