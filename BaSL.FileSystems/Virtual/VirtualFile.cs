using System.IO;

namespace BaSL.FileSystems.Virtual;

internal sealed class VirtualFile : File
{

    public override Path FullPath { get; }

    public override Stream Open() => throw new System.NotImplementedException();

}
