using System;
using BaSL.Users;

namespace BaSL.FileSystems;

public sealed class Inode
{

    public Inode(User owner, Modes modes)
    {
        Owner = owner;
        ChangeMode(modes);
    }

    public User Owner { get; internal set; }

    public Mode OwnerMode { get; internal set; }

    public Mode GroupMode => throw new NotImplementedException();

    public Mode OthersMode { get; internal set; }

    internal void ChangeMode(Modes modes)
    {
        OwnerMode = modes.Owner;
        OthersMode = modes.Others;
    }

}
