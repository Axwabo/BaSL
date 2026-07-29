using System;
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

        public static ShellStatement operator |(ShellStatement source, Path executablePath)
            => source is RedirectStatement
                ? source
                : new PipeStatement(source, executablePath);

    }

}
