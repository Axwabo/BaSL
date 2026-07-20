using BaSL.FileSystems;

namespace BaSL.Users;

public sealed class User
{

    internal User(string username) => Username = username;

    public string Username { get; }

    public Path Home { get; internal set; }

    public bool IsSuperuser { get; internal init; }

}
