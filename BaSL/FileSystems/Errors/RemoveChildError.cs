namespace BaSL.FileSystems.Errors;

public abstract record RemoveChildError(string Message) : FileSystemError(Message)
{

    public static RemoveChildError NothingToRemove { get; } = new NothingToRemoveError();

    public static RemoveChildError DirectoryNotEmpty { get; } = new DirectoryNotEmptyError();

}

public sealed record NothingToRemoveError() : RemoveChildError("Nothing to remove");

public sealed record DirectoryNotEmptyError() : RemoveChildError("Directory is not empty");
