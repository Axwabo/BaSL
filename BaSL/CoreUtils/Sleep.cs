using System;
using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.Executables.Attributes;

namespace BaSL.CoreUtils;

[Help("Sleeps for the given amount of seconds (incredibly inaccurate for some reason).")]
public sealed partial class Sleep : App
{

    [Execute]
    private static async Task<int> SleepAsync(int? seconds, CancellationToken cancellationToken)
    {
        if (seconds == null)
            return 1;
        await SleepMs(seconds.Value * 1000, cancellationToken);
        return 0;
    }

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public static Func<int, CancellationToken, Task> SleepMs { get; set; } = Task.Delay;

    public Sleep(ExecutableContext context) : base(context)
    {
    }

}
