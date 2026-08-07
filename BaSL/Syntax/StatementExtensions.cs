using System;
using System.Collections.Generic;
using BaSL.FileSystems;

namespace BaSL.Syntax;

public static class StatementExtensions
{

    extension(ShellStatement)
    {

        public static ShellStatement? operator >(ShellStatement? source, Path sinkPath)
            => source switch
            {
                ExtendableStatement extendable => new RedirectStatement(extendable, sinkPath, true),
                RedirectStatement {Source: var redirectSource} => new RedirectStatement(redirectSource, sinkPath, true),
                _ => null
            };

        public static ShellStatement? operator <(ShellStatement? source, Path sourcePath)
            => source switch
            {
                StandaloneStatement statement => new FileStdinStatement(statement.Location, statement.Args, sourcePath),
                FileStdinStatement {Location: var location, Args: var args} => new FileStdinStatement(location, args, sourcePath),
                _ => null
            };

        public static ShellStatement? operator >> (ShellStatement? source, Path sinkPath)
            => source switch
            {
                ExtendableStatement extendable => new RedirectStatement(extendable, sinkPath, false),
                RedirectStatement {Source: var redirectSource} => new RedirectStatement(redirectSource, sinkPath, false),
                _ => null
            };

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
