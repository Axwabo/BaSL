using System;

namespace BaSL.FileSystems.Extensions;

public static class PathExtensions
{

    extension(Path)
    {

        // ReSharper disable once InvokeAsExtensionMemberFromSameClass
        public static Path ToAbsolutePath(string path, Directory basePath) => ToAbsolutePath(path, basePath.FullPath);

        public static Path ToAbsolutePath(string path, string basePath) => new Path(path).ToAbsolute(basePath);

        public static Path ToAbsolutePath(string path, Path basePath) => new Path(path).ToAbsolute(basePath);

    }

    extension(Path path)
    {

        public Path Parent
        {
            get
            {
                var pathSpan = path.Value.AsSpan();
                var slash = pathSpan.LastIndexOf('/');
                return slash != -1
                    ? pathSpan[..slash].ToString()
                    : new Path();
            }
        }

        public FileSystemEntryName Name
        {
            get
            {
                var pathSpan = path.Value.AsSpan();
                var slash = pathSpan.LastIndexOf('/');
                return slash != -1
                    ? pathSpan[(slash + 1)..].ToString()
                    : path.Value;
            }
        }

        public bool IsEmpty => string.IsNullOrEmpty(path.Value);

    }

    extension(ReadOnlyMemory<string> memory)
    {

        public Path FirstOrDefault(Path other) => memory.Length == 0 ? other : memory.Span[0];

    }

}
