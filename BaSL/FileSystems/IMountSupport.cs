using BaSL.Users;

namespace BaSL.FileSystems;

internal interface IMountSupport
{

    CreateDirectoryResult Mount(UserContext context, FileSystem fileSystem, FileSystemEntryName name);

}
