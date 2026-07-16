namespace BaSL.FileSystems.Tests;

public sealed class VfsTest
{

    private readonly FileSystem _fs = FileSystem.CreateVirtual();

    [Fact]
    public void CreateDirectoryInRootThrows() => Assert.Throws<IOException>(() => _fs.Root.CreateDirectory("amogus"));

    [Fact]
    public void CreateFileInRootThrows() => Assert.Throws<IOException>(() => _fs.Root.CreateFile("amogus"));

}
