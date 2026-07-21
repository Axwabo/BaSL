using BaSL.Users;

namespace BaSL.FileSystems;

internal interface IMountSupport
{

    CreateDirectoryResult Mount(FileSystem fileSystem, FileSystemEntryName name, User owner, Modes modes);

}
