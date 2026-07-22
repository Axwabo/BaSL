using System.IO;

namespace BaSL.Executables.Pipes;

public readonly record struct Streams(Stream? StandardInput, Stream? StandardOutput, Stream? StandardError);
