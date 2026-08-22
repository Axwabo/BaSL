#:property TargetFramework=net10.0

using System.IO.Compression;

const string licenses = "THIRD_PARTY_LICENSES";
const string readme = "README.md";

using var zipFile = File.Create(args[0]);
using var archive = new ZipArchive(zipFile, ZipArchiveMode.Create);

var executable = Directory.EnumerateFiles(".", "BaSL.Terminal*").First();
archive.CreateEntryFromFile(executable, $"bin/{Path.GetFileName(executable)}");
archive.CreateEntryFromFile($"../{readme}", readme);

foreach (var file in Directory.EnumerateFiles($"../{licenses}"))
    archive.CreateEntryFromFile(file, $"{licenses}/{Path.GetFileName(file)}");
