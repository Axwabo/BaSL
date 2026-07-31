using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BaSL.FileSystems;

namespace BaSL.Syntax;

public static class PipingExtensions
{

    extension(ShellStatement)
    {

        public static ShellStatement operator >(ShellStatement source, Path sinkPath)
            => new RedirectStatement(source, sinkPath, true);

        public static ShellStatement operator <(ShellStatement source, Path sinkPath)
            => throw new NotImplementedException();

        public static ShellStatement operator >> (ShellStatement source, Path sinkPath)
            => new RedirectStatement(source, sinkPath, false);

        [return: NotNullIfNotNull(nameof(source))] // TODO
        public static ShellStatement? operator |(ShellStatement? source, Path executablePath)
            => source is RedirectStatement or null
                ? source
                : new PipeStatement(source, executablePath);

    }

    extension(StandaloneStatement)
    {

        public static StandaloneStatement? FromArgs(List<string> args)
            => args.Count == 0
                ? null
                : new StandaloneStatement(args[0], args.ToArray().AsMemory()[1..]);

        public static StandaloneStatement? FromArgs(ReadOnlyMemory<string> args)
            => args.Length == 0
                ? null
                : new StandaloneStatement(args.Span[0], args[1..]);

    }

}
