using BaSL.FileSystems;
using BaSL.FileSystems.Errors;

// ReSharper disable NotAccessedPositionalProperty.Global

namespace BaSL.CoreUtils;

public abstract record RemoveError(Path EntryPath, string Message);

public sealed record CannotRemoveDirectory(Path EntryPath) : RemoveError(EntryPath, "Is a directory");

public sealed record CannotRemove(Path EntryPath, RemoveEntryError Error) : RemoveError(EntryPath, Error.Message);

public sealed record ParentDirectoryNotFound(Path EntryPath, GetEntryError Error) : RemoveError(EntryPath, $"Cannot find parent directory: {Error.Message}");
