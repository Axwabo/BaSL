using System;
using BaSL.FileSystems;

namespace BaSL.Syntax;

public abstract record ShellStatement;

public sealed record PipeStatement(ShellStatement Source, CommandLocation TargetLocation, ReadOnlyMemory<string> TargetArgs = default) : ShellStatement;

public sealed record StandaloneStatement(CommandLocation Location, ReadOnlyMemory<string> Args = default) : ShellStatement;

public sealed record RedirectStatement(ShellStatement Source, Path SinkPath, bool Overwrite) : ShellStatement;
