using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.FileSystems;
using BaSL.FileSystems.Errors;
using BaSL.FileSystems.Extensions;
using BaSL.Syntax;

namespace BaSL.Interpreter;

using LocateCommandResult = Result<RunCommand, Error>;

internal delegate Result<Task<int>, Error> RunCommand(ExecutableContext context, CancellationToken cancellationToken);

internal static class Run
{

    private static RunCommand Execute(File file) => (context, token) =>
    {
        var execute = file.Execute(context, token);
        return !execute.Success
            ? execute.Error
            : Result<Task<int>, Error>.CreateSuccess(RunAndComplete(context, execute.Value));
    };

    private static async Task<int> RunAndComplete(ExecutableContext context, Process process)
    {
        var code = await process.WaitForExitAsync();
        await context.CompletePipesAsync();
        return code;
    }

    public static LocateCommandResult Locate(BaShell shell, CommandLocation location) => location switch
    {
        PathCommandLocation {FullPath: var path} => Execute(shell, path),
        AutoCommandLocation {Phrase: var path} when Path.IsExplicitRelativeOrAbsolute(path) => Execute(shell, path),
        AutoCommandLocation {Phrase: var name} when BaShell.BuiltInCommands.TryGetValue(name, out var action) => LocateCommandResult.CreateSuccess((context, token) => RunAndComplete(context, Process.Start(action, context, token))),
        AutoCommandLocation {Phrase: var name} => shell.ResolveFromPath(name) switch
        {
            {Success: true, Value: var file} => Execute(file),
            {Success: false, Error: NotFoundError} => CommandNotFoundError.Instance,
            {Error: var error} => error
        },
        _ => CommandNotFoundError.Instance
    };

    private static LocateCommandResult Execute(BaShell shell, Path path)
    {
        var file = shell.CurrentDirectory.ResolveFile(path);
        return file.Success ? Execute(file.Value) : file.Error;
    }

    public static Result<Task<int>, Error> Execute(BaShell shell, CommandLocation location, ExecutableContext context, CancellationToken cancellationToken)
    {
        var locate = Locate(shell, location);
        return locate.Success ? locate.Value(context, cancellationToken) : locate.Error;
    }

}
