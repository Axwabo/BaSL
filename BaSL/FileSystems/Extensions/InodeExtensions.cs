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

    }

}
