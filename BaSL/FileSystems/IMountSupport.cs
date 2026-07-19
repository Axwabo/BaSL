namespace BaSL.FileSystems;

internal interface IMountSupport
{

    Directory Mount(FileSystem fileSystem, FileSystemEntryName name);

}
