using BaSL.CoreUtils;
using BaSL.FileSystems.Extensions;

namespace BaSL.Shell.Console;

public static class Setup
{

    private const string Username = "user";

    private const string Shebang = """
                                   #!/usr/bin/basl
                                   echo Hello from subshell!
                                   if [[ "$USER" == "root" ]]
                                       echo You are root
                                   else
                                       echo You are NOT root
                                   fi
                                   echo End of if-else statement
                                   """;

    private static async Task<OperatingSystem> CreateSystemAsync(StreamWriter err, string[] args)
    {
        var system = new OperatingSystem {Hostname = "OwOS"};
        await system.InstallCoreUtilsAsync();
        var user = system.CreateUser(Username).Unwrap();
        await system.SudoAsync(async (operatingSystem, context) =>
        {
            await AutoMount.Mount(args, operatingSystem, context, err);
            var userHome = operatingSystem.FileSystem.ResolveDirectory(user.Home).Unwrap();
            await using (var writer = userHome.OpenTextWrite("amogus.txt", context).Unwrap())
            {
                await writer.WriteLineAsync("Hello World!");
            }

            var shebang = userHome.CreateFile(context, "shebang.sh").Unwrap();
            await using (var writer = shebang.OpenTextWrite(context).Unwrap())
            {
                await writer.WriteAsync(Shebang);
            }

            shebang.ChmodPlusX(context);
        });
        return system;
    }

    public static async Task<BaSL.Console> CreateConsoleAsync(string[] args, StreamWriter stdout, StreamWriter stderr)
        => new(await CreateSystemAsync(stderr, args), Username, stdout, stderr);

}
