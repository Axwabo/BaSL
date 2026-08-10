using BaSL.FileSystems.Errors;
using BaSL.Users;

namespace BaSL.FileSystems.Extensions;

public static class FileExtensions
{

    extension(File file)
    {

        public ChangeModeError? ChmodPlusX(UserContext context)
        {
            var (owner, group, others) = file.Metadata.Modes;
            return file.Metadata.ChangeMode(context, new Modes(owner | Mode.Execute, group | Mode.Execute, others | Mode.Execute));
        }

    }

}
