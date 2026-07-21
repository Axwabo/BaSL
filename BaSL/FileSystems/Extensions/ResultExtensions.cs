using BaSL.FileSystems.Errors;

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

}
