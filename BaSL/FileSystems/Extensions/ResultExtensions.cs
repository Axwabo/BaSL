using System.IO;
using System.Threading;
using BaSL.Executables;
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

    extension(GetDirectoryResult result)
    {

        public GetEntryResult AsEntry() => result.Success ? result.Value : result.Error;

    }

    extension(GetFileResult result)
    {

        public StreamReader OpenReadOrNull(UserContext context)
        {
            if (!result.Success)
                return StreamReader.Null;
            var open = result.Value.Open(context, OpenMode.Read);
            return open.Success ? new StreamReader(open.Value) : StreamReader.Null;
        }

        public Result<Process, FileSystemError> Execute(ExecutableContext context, CancellationToken cancellationToken)
        {
            if (!result.Success)
                return result.Error;
            var execute = result.Value.Execute(context, cancellationToken);
            return execute.Success ? execute.Value : execute.Error;
        }

    }

    extension<T>(Result<File, T> result) where T : FileSystemError
    {

        public Result<Stream, FileSystemError> Open(UserContext context, OpenMode mode = OpenMode.Read)
        {
            if (!result.Success)
                return result.Error;
            var open = result.Value.Open(context, mode);
            return open.Success ? open.Value : open.Error;
        }

        public Result<Stream, FileSystemError> OpenRead(UserContext context) => result.Open(context);

        public Result<Stream, FileSystemError> OpenWrite(UserContext context) => result.Open(context, OpenMode.ReadWrite);

        public Result<StreamWriter, FileSystemError> OpenTextWrite(UserContext context)
        {
            var open = result.OpenWrite(context);
            return open.Success ? new StreamWriter(open.Value) : open.Error;
        }

    }

    extension(CreateDirectoryResult result)
    {

        public CreateDirectoryResult CreateDirectory(UserContext context, FileSystemEntryName name)
            => result.Success
                ? result.Value.CreateDirectory(context, name)
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

        public FileSystemError? MakeExecutable(UserContext context, Executable executable)
            => result.Success ? result.Value.MakeExecutable(context, executable) : result.Error;

    }

}
