using System;
using BaSL.Users;

namespace BaSL.FileSystems;

public sealed class Inode
{

    internal Inode(User owner, Modes modes)
    {
        Owner = owner;
        ChangeMode(modes);
    }

    public User Owner { get; private set; }

    public Mode OwnerMode { get; private set; }

    public Mode GroupMode => throw new NotImplementedException();

    public Mode OthersMode { get; private set; }

    public Modes Modes => new(OwnerMode, 0, OthersMode);

    internal bool IsFrozen { get; init; }

    internal void ChangeOwner(User user)
    {
        if (!IsFrozen)
            Owner = user;
    }

    internal void ChangeMode(Modes modes)
    {
        if (IsFrozen)
            return;
        OwnerMode = modes.Owner;
        OthersMode = modes.Others;
    }

}
