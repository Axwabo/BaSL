namespace BaSL.Interpreter;

public sealed record CommandNotFoundError() : Error("Command not found")
{

    public static Error Instance { get; } = new CommandNotFoundError();

}
