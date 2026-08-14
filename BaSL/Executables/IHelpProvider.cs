using System.Threading;
using System.Threading.Tasks;

namespace BaSL.Executables;

public interface IHelpProvider
{

    // if you close a pipe here, shame on you
    // TODO: probably use -h instead
    Task DisplayHelpAsync(CancellationToken cancellationToken);

}
