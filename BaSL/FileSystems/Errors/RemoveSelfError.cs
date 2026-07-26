

// ReSharper disable NotAccessedPositionalProperty.Global

namespace BaSL.FileSystems.Errors;

public abstract record RemoveSelfError(Path EntryPath, string Message) : Error(Message);

public sealed record CannotRemoveDirectory(Path EntryPath) : RemoveSelfError(EntryPath, "Is a directory");

public sealed record CannotRemoveSelf(Path EntryPath, RemoveChildError Error) : RemoveSelfError(EntryPath, Error.Message);

public sealed record ParentDirectoryNotFound(Path EntryPath, GetEntryError Error) : RemoveSelfError(EntryPath, $"Cannot find parent directory: {Error.Message}");
