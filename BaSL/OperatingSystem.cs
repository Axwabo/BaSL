using System;
using System.Collections.Generic;
using BaSL.FileSystems;
using BaSL.FileSystems.Extensions;
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
            Home = Path.Root, // TODO: probably..?
            Environment =
            {
                {"PATH", Path.Binaries.Value}
            }
        };
        Users["root"] = Root;
        FileSystem = FileSystem.CreateVirtual(Root);
        _homes = (IMountSupport) FileSystem.Root.CreateDirectory(new UserContext(Root), "home").Unwrap();
    }

    internal User Root { get; }

    internal Dictionary<string, User> Users { get; } = [];

    public FileSystem FileSystem { get; }

    public string Hostname { get; set; } = Guid.NewGuid().ToString("N").ToUpper();

    public CreateUserResult CreateUser(string name)
    {
        if (!FileSystemEntryName.IsValid(name))
            return CreateUserError.InvalidUsername;
        if (Users.ContainsKey(name))
            return CreateUserError.Exists;
        var user = new User(name)
        {
            Environment =
            {
                {"PATH", Path.Binaries.Value}
            }
        };
        var userFs = FileSystem.CreateVirtual(user);
        var mount = _homes.Mount(new UserContext(Root), userFs, name);
        if (!mount.Success)
            return new CannotMountHome(mount.Error);
        user.Home = mount.Value.FullPath;
        Users[name] = user;
        return user;
    }

}
