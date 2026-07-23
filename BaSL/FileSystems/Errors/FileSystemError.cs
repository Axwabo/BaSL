namespace BaSL.FileSystems.Errors;

public abstract record FileSystemError(string Message) : Error(Message);
