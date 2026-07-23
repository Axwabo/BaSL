namespace BaSL.Shell.Console;

public static class SystemConsole
{

    // TODO: eradicate System.
    public static (StreamReader In, StreamWriter Out, StreamWriter Err) Open()
    {
        var stdin = System.Console.OpenStandardInput();
        var stdout = System.Console.OpenStandardOutput();
        var stderr = System.Console.OpenStandardError();
        var inReader = new StreamReader(stdin);
        var outWriter = new StreamWriter(stdout) {AutoFlush = true};
        var errWriter = new StreamWriter(stderr) {AutoFlush = true};
        System.Console.SetIn(inReader);
        System.Console.SetOut(outWriter);
        System.Console.SetError(errWriter);
        return (inReader, outWriter, errWriter);
    }

}
