namespace BaSL.FileSystems.Errors;

public abstract record ChangeModeError(string Message) : Error(Message)
{

    public static ChangeModeError AccessDenied { get; } = new ChangeModeDenied();

    public static ChangeModeError Immutable { get; } = new ImmutableFileError();

}

public sealed record ChangeModeDenied() : ChangeModeError("Access denied");

public sealed record ImmutableFileError() : ChangeModeError("File is immutable");
