using System.Collections.Generic;
using BaSL.FileSystems;
using BaSL.Users;

namespace BaSL;

public sealed class OperatingSystem
{

    private readonly IMountSupport _homes;

    public OperatingSystem()
    {
        Root = new User("root")
        {
            IsSuperuser = true,
            Home = Path.Root // TODO: probably..?
        };
        Users["root"] = Root;
        FileSystem = FileSystem.CreateVirtual(Root);
        _homes = (IMountSupport) FileSystem.Root.CreateDirectory("home").Value!;
    }

    internal User Root { get; }

    internal Dictionary<string, User> Users { get; } = [];

    public FileSystem FileSystem { get; }

    public User CreateUser(string name)
    {
        FileSystemEntryName entryName = name;
        var user = new User(name);
        Users.Add(name, user);
        var userFs = FileSystem.CreateVirtual(user);
        var mount = _homes.Mount(new UserContext(Root), userFs, entryName).Value!;
        user.Home = mount.FullPath;
        return user;
    }

}
