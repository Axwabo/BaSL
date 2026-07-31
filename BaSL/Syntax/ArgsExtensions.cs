using System;
using BaSL.FileSystems;
using static BaSL.Syntax.StatementExtensions;

// ReSharper disable InvokeAsExtensionMember

namespace BaSL.Syntax;

public static class ArgsExtensions
{

    extension(Args)
    {

        public static ShellStatement? operator >(Args source, Path sinkPath) => FromArgs(source) > sinkPath;

        public static ShellStatement operator <(Args source, Path sinkPath) => throw new NotImplementedException();

        public static ShellStatement? operator >> (Args source, Path sinkPath) => FromArgs(source) >> sinkPath;

        public static ShellStatement? operator |(Args source, Path executablePath) => FromArgs(source) | executablePath;

        public static ShellStatement? operator |(Args source, StandaloneStatement? target) => FromArgs(source) | target;

        public static ShellStatement? operator |(Args source, Args target) => source | FromArgs(target);

    }

}
