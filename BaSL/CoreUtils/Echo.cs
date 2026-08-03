using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;

namespace BaSL.CoreUtils;

public sealed class Echo : App
{

    private static char GetEscaped(char c) => c switch
    {
        'n' => '\n',
        't' => '\t',
        '\\' => '\\',
        _ => char.MinValue
    };

    public Echo(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        var args = Args;
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            await WriteAsync(cancellationToken, arg);
            if (i != args.Length - 1)
                await StandardOutput.WriteAsync(" ", cancellationToken);
        }

        await StandardOutput.WriteLineAsync();
        return 0;
    }

    private async Task WriteAsync(CancellationToken cancellationToken, string arg)
    {
        var buffer = ArrayPool<char>.Shared.Rent(arg.Length);
        try
        {
            var writeIndex = 0;
            for (var i = 0; i < arg.Length; i++)
            {
                var c = arg[i];
                if (c == '\\' && i < arg.Length - 1)
                {
                    if (GetEscaped(arg[i + 1]) is not char.MinValue and var escaped)
                    {
                        buffer[writeIndex++] = escaped;
                        i++;
                    }

                    continue;
                }

                buffer[writeIndex++] = c;
            }

            await StandardOutput.WriteAsync(buffer.AsMemory(0, writeIndex), cancellationToken);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }

}
