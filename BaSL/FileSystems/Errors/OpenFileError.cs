namespace BaSL.FileSystems.Errors;

public abstract record OpenFileError(string Message) : FileSystemError(Message)
{

    public static OpenFileError AccessDenied { get; } = new OpenDeniedError();

    public static OpenFileError AccessViolation { get; } = new AccessViolationError();

}

public sealed record OpenDeniedError() : OpenFileError("Access denied");

public sealed record AccessViolationError() : OpenFileError("Access violation");
