#:property TargetFramework=net10.0

using System.IO.Compression;

using var zipFile = File.Create(args[0]);
using var archive = new ZipArchive(zipFile, ZipArchiveMode.Create);

var executable = Directory.EnumerateFiles(".", "BaSL.Shell.Console>").First();
archive.CreateEntryFromFile($"bin/{Path.GetFileName(executable)}", executable);
archive.CreateEntryFromFile("../README.md", "README.md");

foreach (var file in Directory.EnumerateFiles("../THIRD_PARTY_LICENSES"))
    archive.CreateEntryFromFile(file, $"THIRD_PARTY_LICENSES/{Path.GetFileName(file)}");