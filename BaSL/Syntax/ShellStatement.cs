using System;
using BaSL.FileSystems;

namespace BaSL.Syntax;

public abstract record ShellStatement;

public sealed record PipeStatement(ShellStatement Source, Path TargetPath, ReadOnlyMemory<string> TargetArgs = default) : ShellStatement;

public sealed record StandaloneStatement(Path FullPath, ReadOnlyMemory<string> Args = default) : ShellStatement;

public sealed record RedirectStatement(ShellStatement Source, Path SinkPath, bool Overwrite) : ShellStatement;
