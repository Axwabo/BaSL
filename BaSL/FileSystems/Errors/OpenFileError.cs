namespace BaSL.FileSystems.Errors;

public abstract record OpenFileError(string Message)
{

    public static OpenFileError AccessDenied { get; } = new OpenDeniedError();

}

public sealed record OpenDeniedError() : OpenFileError("Access denied");
