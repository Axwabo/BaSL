using System;
using System.Collections.Generic;

// ReSharper disable InvokeAsExtensionMemberFromSameClass

namespace BaSL.FileSystems.Extensions;

public static class PathExtensions
{

    extension(Path)
    {

        // ReSharper disable once InvokeAsExtensionMemberFromSameClass
        public static Path ToPartialAbsolutePath(string path, Directory basePath) => ToPartialAbsolutePath(path, basePath.FullPath);

        public static Path ToPartialAbsolutePath(string path, string basePath) => new Path(path).ToPartialAbsolute(basePath);

        public static Path ToPartialAbsolutePath(string path, Path basePath) => new Path(path).ToPartialAbsolute(basePath);

        public static bool IsExplicitRelativeOrAbsolute(ReadOnlySpan<char> span) => span.StartsWith("/") || span.StartsWith("./") || span.StartsWith("../");

        public static ReadOnlySpan<char> GetParent(ReadOnlySpan<char> span)
        {
            var slash = span.LastIndexOf('/');
            return slash != -1 ? span[..slash] : default;
        }

        public static ReadOnlySpan<char> GetName(ReadOnlySpan<char> span)
        {
            var slash = span.LastIndexOf('/');
            return slash != -1 ? span[(slash + 1)..] : default;
        }

        // TODO: doesn't work xd
        public static ReadOnlySpan<char> GetCommonAncestor(ReadOnlySpan<char> partialPath, ReadOnlySpan<char> basePath)
        {
            if (!partialPath.StartsWith(basePath))
                return default;
            if (partialPath.SequenceEqual(basePath))
                return partialPath;
            var previous = 0;
            while (true)
            {
                var partialSlash = partialPath.IndexOf('/', previous);
                var baseSlash = basePath.IndexOf('/', previous);
                if (partialSlash != baseSlash)
                    return partialPath[..previous];
                if (!partialPath[previous..partialSlash].SequenceEqual(basePath[previous..baseSlash]))
                    return default;
                previous = partialSlash;
            }
        }

        public static string RemoveRelativeSegments(string path)
        {
            var list = new List<string>();
            var absolute = path.StartsWith('/');
            if (absolute)
                list.Add("");
            foreach (var s in path.Split("/", StringSplitOptions.RemoveEmptyEntries))
            {
                if (s == ".")
                    continue;
                if (s != "..")
                {
                    list.Add(s);
                    continue;
                }

                if (list.Count > (absolute ? 1 : 0))
                    list.RemoveAt(list.Count - 1);
            }

            return absolute && list.Count <= 1 ? "/" : string.Join("/", list);
        }

    }

    extension(Path path)
    {

        public Path Parent => new(GetParent(path.Value.AsSpan()).ToString());

        public FileSystemEntryName Name
        {
            get
            {
                var name = GetName(path.Value.AsSpan());
                return name.IsEmpty ? path.Value : name.ToString();
            }
        }

        public bool IsEmpty => string.IsNullOrEmpty(path.Value);

        public bool IsAbsolute => path.Value.AsSpan().StartsWith(Path.Root.Value);

    }

    extension(ReadOnlyMemory<string> memory)
    {

        public Path FirstOrDefault(Path other) => memory.Length == 0 ? other : memory.Span[0];

    }

}
