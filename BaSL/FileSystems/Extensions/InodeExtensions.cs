using BaSL.FileSystems.Errors;
using BaSL.Users;

namespace BaSL.FileSystems.Extensions;

public static class InodeExtensions
{

    extension(Inode inode)
    {

        public bool CanRead(User user) => inode.HasMode(user, Mode.Read);

        public bool CanWrite(User user) => inode.HasMode(user, Mode.Write);

        public bool CanExecute(User user) => inode.HasMode(user, Mode.Execute);

        public bool HasMode(User user, Mode mode) => user.IsSuperuser || (user == inode.Owner ? inode.Modes.Owner : inode.Modes.Others).Has(mode);

        public ChangeModeError? Add(UserContext context, Modes modes)
        {
            var (owner, group, others) = inode.Modes;
            return inode.ChangeMode(context, new Modes(owner | modes.Owner, group | modes.Group, others | modes.Others));
        }

        public ChangeModeError? Add(UserContext context, Mode mode) => inode.Add(context, new Modes(mode, mode, mode));

    }

}
