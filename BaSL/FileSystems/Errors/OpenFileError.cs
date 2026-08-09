namespace BaSL.FileSystems.Errors;

public abstract record OpenFileError(string Message) : FileSystemError(Message)
{

    public static OpenFileError AccessDenied { get; } = new OpenDeniedError();

    public static OpenFileError AccessViolation { get; } = new AccessViolationError();

    public static OpenFileError NotExecutable { get; } = new NotExecutableError();

    public static OpenFileError ShebangNotFound { get; } = new ShebangExecutableNotFoundError();

}

public sealed record OpenDeniedError() : OpenFileError("Access denied");

public sealed record AccessViolationError() : OpenFileError("Access violation");

public sealed record NotExecutableError() : OpenFileError("File is not executable");

public sealed record ShebangExecutableNotFoundError() : OpenFileError("Cannot find shebang-specified program");

// ReSharper disable once NotAccessedPositionalProperty.Global
public sealed record ShebangExecutionFailedError(Error Error) : OpenFileError($"Cannot execute shebang-specified program: {Error.Message}");
