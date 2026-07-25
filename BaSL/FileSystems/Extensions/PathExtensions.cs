using System;

namespace BaSL.FileSystems.Extensions;

public static class PathExtensions
{

    extension(Path)
    {

        public static Path ToAbsolutePath(string path, Directory basePath) => Path.ToAbsolutePath(path, basePath.FullPath);

    }

    extension(ReadOnlyMemory<string> memory)
    {

        public Path FirstOrDefault(Path other) => memory.Length == 0 ? other : memory.Span[0];

    }

}
