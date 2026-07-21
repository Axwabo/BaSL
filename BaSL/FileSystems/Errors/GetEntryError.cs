namespace BaSL.FileSystems.Errors;

public abstract record GetEntryError(string Message)
{

    public static GetEntryError NotFound { get; } = new NotFoundError();

    public static GetEntryError NotAFile { get; } = new NotAFile();

    public static GetEntryError NotADirectory { get; } = new NotADirectory();

}

public sealed record NotFoundError() : GetEntryError("Entry not found");

public sealed record NotAFile() : GetEntryError("Not a file");

public sealed record NotADirectory() : GetEntryError("Not a directory");
