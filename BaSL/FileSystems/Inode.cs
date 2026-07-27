using System;
using BaSL.FileSystems.Errors;
using BaSL.FileSystems.Extensions;
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

    private void ChangeMode(Modes modes)
    {
        OwnerMode = modes.Owner;
        OthersMode = modes.Others;
    }

    public ChangeModeError? ChangeMode(UserContext context, Modes modes)
    {
        if (IsFrozen)
            return ChangeModeError.Immutable;
        if (context.User != Owner && !this.CanWrite(context))
            return ChangeModeError.AccessDenied;
        ChangeMode(modes);
        return null;
    }

}
