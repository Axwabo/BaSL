using System.Threading.Tasks;

namespace BaSL.Executables;

public delegate Task<int> Executable(ExecutableContext context);
