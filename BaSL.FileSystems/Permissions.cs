using System;

namespace BaSL.FileSystems;

// TODO: 777 or something
[Flags]
public enum Permissions
{

    None = 0,
    Read = 1 << 0,
    Write = 1 << 1,
    Execute = 1 << 2,
    Rw = Read | Write,
    Rwx = Read | Write | Execute

}

public static class PermissionsExtensions
{

    extension(Permissions permissions)
    {

        public bool CanRead => permissions.Has(Permissions.Read);

        public bool CanWrite => permissions.Has(Permissions.Write);

        public bool CanExecute => permissions.Has(Permissions.Execute);

        public bool Has(Permissions other) => (permissions & other) == other;

    }

}
