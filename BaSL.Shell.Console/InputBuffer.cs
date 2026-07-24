using System.Text;

namespace BaSL.Shell.Console;

public static class InputBuffer
{

    public static async Task ReadAsync(StreamWriter consoleStandardInput, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var sb = new StringBuilder();
                while (!cancellationToken.IsCancellationRequested)
                {
                    var key = await ReadKeyAsync(cancellationToken);
                    if (key.Key == ConsoleKey.Enter)
                    {
                        System.Console.WriteLine();
                        break;
                    }

                    if (key.Key != ConsoleKey.Backspace)
                    {
                        sb.Append(key.KeyChar);
                        System.Console.Write(key.KeyChar);
                    }
                    else if (sb.Length != 0)
                    {
                        var pos = --System.Console.CursorLeft;
                        System.Console.Write(' ');
                        System.Console.CursorLeft = pos;
                        sb.Remove(sb.Length - 1, 1);
                    }
                }

                await consoleStandardInput.WriteLineAsync(sb.ToString());
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static async Task<ConsoleKeyInfo> ReadKeyAsync(CancellationToken cancellationToken)
    {
        while (!System.Console.KeyAvailable)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Yield();
        }

        return System.Console.ReadKey(true);
    }

}
