using System.IO;

namespace BaSL.FileSystems;

public abstract class File : FileSystemEntry
{

    public abstract Stream Open();

}
