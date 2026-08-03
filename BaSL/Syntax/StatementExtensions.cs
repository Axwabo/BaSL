using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BaSL.FileSystems;

namespace BaSL.Syntax;

public static class StatementExtensions
{

    extension(ShellStatement)
    {

        [return: NotNullIfNotNull(nameof(source))]
        public static ShellStatement? operator >(ShellStatement? source, Path sinkPath)
            => source is not ExtendableStatement extendable
                ? null
                : new RedirectStatement(extendable, sinkPath, true);

        public static ShellStatement operator <(ShellStatement source, Path sinkPath)
            => throw new NotImplementedException();

        [return: NotNullIfNotNull(nameof(source))]
        public static ShellStatement? operator >> (ShellStatement? source, Path sinkPath)
            => source is not ExtendableStatement extendable
                ? null
                : new RedirectStatement(extendable, sinkPath, false);

        [return: NotNullIfNotNull(nameof(source))]
        public static ShellStatement? operator |(ShellStatement? source, Path executablePath)
            => source is not ExtendableStatement extendable
                ? source
                : new PipeStatement(extendable, executablePath);

        public static ShellStatement? operator |(ShellStatement? source, StandaloneStatement? target)
            => target is null || source is not ExtendableStatement extendable
                ? source
                : new PipeStatement(extendable, target.Location, target.Args);

    }

    extension(StandaloneStatement)
    {

        public static StandaloneStatement? FromArgs(List<string> args)
            => args.Count == 0
                ? null
                : new StandaloneStatement(args[0], args.ToArray().AsMemory(1));

        public static StandaloneStatement? FromArgs(ReadOnlyMemory<string> args)
            => args.Length == 0
                ? null
                : new StandaloneStatement(args.Span[0], args[1..]);

        public static StandaloneStatement? FromArgs(Args args) => StandaloneStatement.FromArgs(args.Value);

        public static StandaloneStatement FromPath(Path fullPath, params string[] args) => new(fullPath, args);

    }

}
