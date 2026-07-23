namespace BaSL.FileSystems.Errors;

public abstract record CreateEntryError(string Message) : FileSystemError(Message)
{

    public static CreateEntryError AccessDenied { get; } = new CreateDeniedError();

    public static CreateEntryError NameCollision { get; } = new NameCollisionError();

    public static CreateEntryError ImmutableFileSystem { get; } = new ImmutableFileSystemError();

}

public sealed record CreateDeniedError() : CreateEntryError("Access denied");

public sealed record NameCollisionError() : CreateEntryError("An entry with the same name already exists");

public sealed record ImmutableFileSystemError() : CreateEntryError("File system is immutable");
