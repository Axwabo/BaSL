namespace BaSL.FileSystems.Extensions;

public static class ModeExtensions
{

    extension(OpenMode mode)
    {

        public bool IsWrite => (mode & OpenMode.Write) != 0;

        public bool IsTruncate => (mode & OpenMode.Truncate) != 0;

    }

}
