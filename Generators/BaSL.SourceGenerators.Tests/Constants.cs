namespace BaSL.SourceGenerators.Tests;

public static class Constants
{

    public const string Source = """
                                 using System.Threading;
                                 using BaSL.Executables;
                                 using BaSL.Executables.Attributes;
                                     
                                 namespace BaSL.CoreUtils;

                                 public sealed partial class Ls : App
                                 {

                                     [Execute]
                                     public async Task<int> MogusAsync(string? path = null, [Flag] bool longFormat = false, CancellationToken token = default)
                                     {
                                         return 0;
                                     }

                                 }
                                 """;

    public const string Result = """
                                 #nullable enable
                                 namespace BaSL.CoreUtils
                                 {
                                     partial class Ls
                                     {
                                         public override async global::System.Threading.Tasks.Task<int> ExecuteAsync(global::System.Threading.CancellationToken cancellationToken)
                                         {string?path = null;
                                 bool? longFormat = false;
                                 int positionalArgumentIndex = 0;
                                 for (int i = 0; i < this.Args.Length; i++)
                                 {
                                 string arg = this.Args[i];
                                 if (arg.StartsWith('-'))
                                 {
                                 for (int c = 1; c < arg.Length; c++)
                                 {
                                 if (c == 'l')
                                 {
                                 longFormat = true;
                                 }
                                 }
                                 continue;
                                 }
                                 if (positionalArgumentIndex == 0)
                                 {
                                 path = arg;
                                 }
                                 positionalArgumentIndex++;
                                 }
                                             return await MogusAsync(path, longFormat.Value, cancellationToken);
                                         }
                                     }
                                 }
                                 """;

}
