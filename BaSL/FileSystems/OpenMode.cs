using System;

namespace BaSL.FileSystems;

[Flags]
public enum OpenMode
{

    Read = 0,
    Write = 1,
    Truncate = 2

}
