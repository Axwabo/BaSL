using System;

namespace BaSL.FileSystems.Extensions;

public static class FileSystemExtensions
{

    extension(FileSystem fileSystem)
    {

        public FileSystemEntry Resolve(Path path)
        {
            var absolute = path.ToAbsolutePath("/");
            var span = absolute.Value.AsSpan();
            var lastIndex = 1;
            do
            {
                var index = span[lastIndex..].IndexOf('/');
                var segment = span.Slice(lastIndex, index == -1 ? span.Length - lastIndex : index);
            }
            while (lastIndex != -1);
        }

    }

}
