using System.IO;
using BaSL.FileSystems.Errors;
using BaSL.Users;

namespace BaSL.FileSystems.Extensions;

public static class FileExtensions
{

    extension(File file)
    {

        public ChangeModeError? ChmodPlusX(UserContext context) => file.Metadata.Add(context, Mode.Execute);

        public OpenFileResult OpenRead(UserContext context) => file.Open(context, OpenMode.Read);

        public OpenFileResult OpenWrite(UserContext context) => file.Open(context, OpenMode.ReadWrite);

        public Result<StreamWriter, OpenFileError> OpenTextWrite(UserContext context)
        {
            var open = file.OpenWrite(context);
            return open.Success ? new StreamWriter(open.Value) : open.Error;
        }

    }

}
