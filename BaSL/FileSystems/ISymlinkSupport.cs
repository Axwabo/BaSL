using BaSL.FileSystems.Errors;
using BaSL.Users;

namespace BaSL.FileSystems;

public interface ISymlinkSupport
{

    Result<SymbolicLink, CreateEntryError> Link(UserContext context, FileSystemEntryName name, Path target);

}
