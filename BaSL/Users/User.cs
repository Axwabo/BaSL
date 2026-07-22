using System.Collections.Generic;
using System.Collections.ObjectModel;
using BaSL.FileSystems;

namespace BaSL.Users;

public sealed class User
{

    internal User(string username)
    {
        Username = username;
        EnvironmentVariables = new ReadOnlyDictionary<string, string>(Environment);
    }

    public string Username { get; }

    public bool IsSuperuser { get; internal init; }

    public Path Home { get; internal set; }

    internal Dictionary<string, string> Environment { get; } = [];

    public IReadOnlyDictionary<string, string> EnvironmentVariables { get; }

}
