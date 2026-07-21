namespace BaSL.FileSystems.Errors;

public abstract record OpenFileError(string Message)
{

    public static OpenFileError AccessDenied { get; } = new AccessDeniedError();

}

public sealed record AccessDeniedError() : OpenFileError("Access denied");
