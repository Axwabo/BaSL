using System;
using BaSL.FileSystems;

namespace BaSL.Syntax;

public abstract record ShellStatement;

public abstract record ExtendableStatement : ShellStatement;

public sealed record StandaloneStatement(CommandLocation Location, ReadOnlyMemory<string> Args = default) : ExtendableStatement;

public sealed record PipeStatement(ExtendableStatement Source, CommandLocation TargetLocation, ReadOnlyMemory<string> TargetArgs = default) : ExtendableStatement;

public sealed record RedirectStatement(ExtendableStatement Source, Path SinkPath, bool Overwrite) : ShellStatement;
