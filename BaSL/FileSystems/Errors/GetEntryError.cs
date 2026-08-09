namespace BaSL.FileSystems.Errors;

public abstract record GetEntryError(string Message) : FileSystemError(Message)
{

    public static GetEntryError NotFound { get; } = new NotFoundError();

    public static GetEntryError NotAFile { get; } = new NotAFile();

    public static GetEntryError NotADirectory { get; } = new NotADirectory();

    public static GetEntryError SymlinkLimit { get; } = new SymlinkLimit();

}

public sealed record NotFoundError() : GetEntryError("Entry not found");

public sealed record NotAFile() : GetEntryError("Not a file");

public sealed record NotADirectory() : GetEntryError("Not a directory");

public sealed record SymlinkLimit() : GetEntryError("Too many symbolic links");
