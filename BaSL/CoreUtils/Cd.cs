using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.Executables.Attributes;
using BaSL.FileSystems;

namespace BaSL.CoreUtils;

[Help("""
      Changes the current directory.
      If no argument is provided, changes to the current user's home directory.
      """)]
public sealed partial class Cd : App
{

    public Cd(ExecutableContext context) : base(context)
    {
    }

    [Execute]
    public Task<int> ChangeAsync([DefaultTo(DefaultDirectory.UserHome)] Directory directory)
    {
        Shell.CurrentDirectory = directory;
        return Task.FromResult(0);
    }

}
