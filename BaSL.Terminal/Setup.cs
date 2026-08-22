using BaSL.CoreUtils;
using BaSL.FileSystems.Extensions;
using Path = BaSL.FileSystems.Path;

namespace BaSL.Terminal;

public static class Setup
{

    private const string Username = "user";

    private const string Banner = """
                                  Welcome to the BaSL console!
                                  This is something like a terminal running bash, but made entirely in .NET!
                                  Syntax and features are rather limited for now.
                                  Type "help" to see available commands.
                                  Type "exit" to exit BaSL.
                                  Some statements (e.g. running "basl") can hang the shell, meaning you'll have to terminate the program (not with Ctrl+C). 
                                  Example commands to try out:
                                  ls
                                  cat amogus.txt | cat | cat | cat
                                  ./shebang.sh
                                  """;

    private const string Prefix = "BaSL.Terminal.Home.";

    private static async Task<OperatingSystem> CreateSystemAsync(StreamWriter err, string[] args)
    {
        var system = new OperatingSystem {Hostname = "OwOS"};
        await system.InstallCoreUtilsAsync();
        var user = system.CreateUser(Username).Unwrap();
        await system.SudoAsync(async (operatingSystem, context) =>
        {
            var bin = operatingSystem.FileSystem.Root.CreateDirectories(context, Path.Binaries).Unwrap();
            bin.CreateBinary(context, "clear", ctx => new Clear(ctx));
            bin.CreateBinary(context, "zipfs", ctx => new ZipFs(ctx));
            await AutoMount.Mount(args, operatingSystem, context, err);
            var userHome = operatingSystem.FileSystem.ResolveDirectory(user.Home).Unwrap();
            var assembly = typeof(Setup).Assembly;
            foreach (var name in assembly.GetManifestResourceNames())
            {
                await using var resource = assembly.GetManifestResourceStream(name);
                if (resource == null)
                    continue;
                var filename = name[Prefix.Length..];
                var file = userHome.CreateFile(context, filename);
                if (filename.EndsWith(".sh"))
                    file.Unwrap().ChmodPlusX(context);
                await using var fileStream = file.OpenWrite(context).Unwrap();
                await resource.CopyToAsync(fileStream);
            }
        });
        return system;
    }

    public static async Task<Console> CreateConsoleAsync(string[] args, StreamWriter stdout, StreamWriter stderr)
    {
        var system = await CreateSystemAsync(stderr, args);
        await stdout.WriteLineAsync(Banner);
        return new Console(system, Username, stdout, stderr);
    }

}
