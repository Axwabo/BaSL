using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
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

        public async Task<OpenFileError?> WriteAllTextAsync(UserContext context, string text, CancellationToken cancellationToken = default)
        {
            var open = file.OpenTextWrite(context);
            if (!open.Success)
                return open.Error;
            await using var writer = open.Value;
            writer.BaseStream.SetLength(0);
            await writer.WriteAsync(text, cancellationToken);
            return null;
        }

    }

}
