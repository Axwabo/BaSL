using System.Text;

namespace BaSL.Shell.Console;

using Console = System.Console; // why...

public static class InputBuffer
{

    private static readonly List<string> PreviousInputs = [];
    private static int _index;

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
                    if (CompleteStatement(key, sb))
                        break;
                }

                await consoleStandardInput.WriteLineAsync(sb.ToString());
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static bool CompleteStatement(ConsoleKeyInfo key, StringBuilder sb)
    {
        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            _index = PreviousInputs.Count;
            PreviousInputs.Add(sb.ToString());
            return true;
        }

        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                Navigate(-1, sb);
                break;
            case ConsoleKey.DownArrow:
                Navigate(1, sb);
                break;
            case ConsoleKey.Backspace when sb.Length == 0:
                break;
            case ConsoleKey.Backspace:
                var pos = --Console.CursorLeft;
                Console.Write(' ');
                Console.CursorLeft = pos;
                sb.Remove(sb.Length - 1, 1);
                break;
            default:
                if (char.IsControl(key.KeyChar))
                    break;
                sb.Append(key.KeyChar);
                Console.Write(key.KeyChar);
                break;
        }

        return false;
    }

    private static void Navigate(int offset, StringBuilder sb)
    {
        var nextIndex = _index + offset;
        if (nextIndex < 0 || nextIndex >= PreviousInputs.Count)
            return;
        _index = nextIndex;
        var text = PreviousInputs[nextIndex];
        var length = sb.Length;
        sb.Clear();
        sb.Append(text);
        Console.CursorLeft -= length;
        Console.Write(text);
        var (left, top) = Console.GetCursorPosition();
        Console.Write(new string(' ', Console.WindowWidth - text.Length - length));
        Console.SetCursorPosition(left, top);
    }

    private static async Task<ConsoleKeyInfo> ReadKeyAsync(CancellationToken cancellationToken)
    {
        while (!Console.KeyAvailable)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(10, cancellationToken);
        }

        return Console.ReadKey(true);
    }

}
