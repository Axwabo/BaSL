using BaSL.FileSystems.Errors;

namespace BaSL.Users;

public abstract record CreateUserError(string Message) : Error(Message)
{

    public static CreateUserError Exists { get; } = new UserAlreadyExists();

    public static CreateUserError InvalidUsername { get; } = new InvalidUsername();

}

public sealed record UserAlreadyExists() : CreateUserError("User with the same name already exists");

public sealed record InvalidUsername() : CreateUserError("Invalid username");

// ReSharper disable once NotAccessedPositionalProperty.Global
public sealed record CannotMountHome(CreateEntryError Error) : CreateUserError($"Cannot mount home directory: {Error.Message}");
