global using CreateDirectoryResult = BaSL.Result<BaSL.FileSystems.Directory, BaSL.FileSystems.Errors.CreateEntryError>;
global using CreateFileResult = BaSL.Result<BaSL.FileSystems.File, BaSL.FileSystems.Errors.CreateEntryError>;
global using GetEntryResult = BaSL.Result<BaSL.FileSystems.FileSystemEntry, BaSL.FileSystems.Errors.GetEntryError>;
global using GetDirectoryResult = BaSL.Result<BaSL.FileSystems.Directory, BaSL.FileSystems.Errors.GetEntryError>;
global using GetFileResult = BaSL.Result<BaSL.FileSystems.File, BaSL.FileSystems.Errors.GetEntryError>;
global using OpenFileResult = BaSL.Result<System.IO.Stream, BaSL.FileSystems.Errors.OpenFileError>;
global using ExecuteFileResult = BaSL.Result<BaSL.Executables.Process, BaSL.FileSystems.Errors.OpenFileError>;
