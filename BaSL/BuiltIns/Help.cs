using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.Executables.Attributes;
using BaSL.FileSystems.Extensions;

namespace BaSL.BuiltIns;

[Help("Displays available commands, or shows the description of the given command if provided.")]
internal sealed partial class Help : BuiltInCommand
{

    public Help(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (Args.IsEmpty)
            return await ListAsync(cancellationToken);
        var result = Shell.ResolveFromPath(Args[0]);
        if (!result.Success)
        {
            await StandardError.WriteLineAsync(result.Error, cancellationToken);
            return 1;
        }

        if (result.Value.Executable is not { } executable)
        {
            await StandardError.WriteLineAsync("Not an executable", cancellationToken);
            return 1;
        }

        await Shell.StandardOutput.WriteLineAsync(result.Value.FullPath.Value);
        var app = executable(Context);
        if (app is IHelpProvider provider)
            await provider.DisplayHelpAsync(cancellationToken);
        else
            await StandardOutput.WriteLineAsync("No help available :(", cancellationToken);
        return 0;
    }

    private async Task<int> ListAsync(CancellationToken cancellationToken)
    {
        var commands = new List<string>(BaShell.BuiltInCommands.Keys);
        foreach (var directoryPath in Shell.PATH)
        {
            var directory = FileSystem.ResolveDirectory(directoryPath);
            if (!directory.Success)
                continue;
            foreach (var file in directory.Value.EnumerateFiles())
                if (file.Executable != null && file.Metadata.CanExecute(UserContext))
                    commands.Add(file.Name.Value);
        }

        commands.Sort(string.Compare);
        foreach (var command in commands)
            await StandardOutput.WriteLineAsync(command, cancellationToken);
        return 0;
    }

}
