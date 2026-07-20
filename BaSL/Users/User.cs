using BaSL.FileSystems;

namespace BaSL.Users;

public sealed class User
{

    internal User(string username, Directory home)
    {
        Username = username;
        Home = home;
    }

    public string Username { get; }

    public Directory Home { get; }

    public bool IsSuperuser { get; internal init; }

}
