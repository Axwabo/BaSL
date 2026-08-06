using BaSL.FileSystems;

namespace BaSL.Syntax;

public abstract record ShellStatement;

public abstract record ExtendableStatement(CommandLocation Location, Args Args) : ShellStatement;

public sealed record StandaloneStatement(CommandLocation Location, Args Args = default) : ExtendableStatement(Location, Args);

public sealed record FileStdinStatement(CommandLocation Location, Args Args, Path SourcePath) : ExtendableStatement(Location, Args);

public sealed record PipeStatement(ExtendableStatement Source, CommandLocation Location, Args Args = default) : ExtendableStatement(Location, Args);

public sealed record RedirectStatement(ExtendableStatement Source, Path SinkPath, bool Overwrite) : ShellStatement;
