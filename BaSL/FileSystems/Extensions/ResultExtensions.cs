using System.IO;
using BaSL.FileSystems.Errors;
using BaSL.Users;

namespace BaSL.FileSystems.Extensions;

public static class ResultExtensions
{

    extension(GetEntryResult result)
    {

        public GetDirectoryResult AsDirectory() => result switch
        {
            {Success: false, Error: var error} => error,
            {Value: Directory directory} => directory,
            _ => GetEntryError.NotADirectory
        };

        public GetFileResult AsFile() => result switch
        {
            {Success: false, Error: var error} => error,
            {Value: File file} => file,
            _ => GetEntryError.NotAFile
        };

    }

    extension(CreateDirectoryResult result)
    {

        public CreateDirectoryResult CreateDirectory(FileSystemEntryName name)
            => result.Success
                ? result.Value.CreateDirectory(name)
                : result;

    }

    extension(CreateFileResult result)
    {

        public Result<Stream, FileSystemError> Open(UserContext context, OpenMode mode)
        {
            if (!result.Success)
                return result.Error;
            var open = result.Value.Open(context, mode);
            return open.Success ? open.Value : open.Error;
        }

    }

}
