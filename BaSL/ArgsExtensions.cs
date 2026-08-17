using BaSL.Executables;
using BaSL.FileSystems;
using BaSL.Syntax;
using static BaSL.Syntax.StatementExtensions;

// ReSharper disable InvokeAsExtensionMember

namespace BaSL;

public static class ArgsExtensions
{

    extension(string[] args)
    {

        public Args AsArgs() => new(args);

    }

    extension(Args args)
    {

        public bool IsEmpty => args.Value.IsEmpty;

        public ReadOnlyMemoryEnumerator<string> GetEnumerator() => args.Value.GetEnumerator();

    }

    extension(Args)
    {

        public static ShellStatement? operator >(Args source, Path sinkPath) => FromArgs(source) > sinkPath;

        public static ShellStatement? operator <(Args source, Path sourcePath) => FromArgs(source) < sourcePath;

        public static ShellStatement? operator >> (Args source, Path sinkPath) => FromArgs(source) >> sinkPath;

        public static ShellStatement? operator |(Args source, Path executablePath) => FromArgs(source) | executablePath;

        public static ShellStatement? operator |(Args source, StandaloneStatement? target) => FromArgs(source) | target;

        public static ShellStatement? operator |(Args source, Args target) => source | FromArgs(target);

        public static ShellStatement? operator |(ShellStatement? statement, Args target) => statement | FromArgs(target);

    }

}
