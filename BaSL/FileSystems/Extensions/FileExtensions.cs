using System;
using BaSL.Executables;

namespace BaSL.FileSystems.Extensions;

public static class FileExtensions
{

    extension(File file)
    {

        public void MakeExecutable(Func<ExecutableContext, Program> createProgram) => file.MakeExecutable(context => createProgram(context).ExecuteAsync());

    }

}
