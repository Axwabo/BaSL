using System;
using System.Threading.Tasks;
using BaSL.Users;

namespace BaSL;

public static class OperatingSystemExtensions
{

    extension(OperatingSystem operatingSystem)
    {

        public async Task SudoAsync(Func<OperatingSystem, UserContext, Task> @do)
            => await @do(operatingSystem, new UserContext(operatingSystem.Root));

    }

}
