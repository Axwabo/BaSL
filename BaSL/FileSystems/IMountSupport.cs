using BaSL.Users;

namespace BaSL.FileSystems;

internal interface IMountSupport
{

    Directory Mount(FileSystem fileSystem, FileSystemEntryName name, User owner, Modes modes);

}
