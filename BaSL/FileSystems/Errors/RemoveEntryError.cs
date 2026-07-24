namespace BaSL.FileSystems.Errors;

public abstract record RemoveEntryError(string Message) : FileSystemError(Message)
{

    public static RemoveEntryError NothingToRemove { get; } = new NothingToRemoveError();

    public static RemoveEntryError DirectoryNotEmpty { get; } = new DirectoryNotEmptyError();

}

public sealed record NothingToRemoveError() : RemoveEntryError("Nothing to remove");

public sealed record DirectoryNotEmptyError() : RemoveEntryError("Directory is not empty");
