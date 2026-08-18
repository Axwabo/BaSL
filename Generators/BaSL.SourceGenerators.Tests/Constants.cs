namespace BaSL.SourceGenerators.Tests;

public static class Constants
{

    public const string Source = """
                                 using BaSL.Executables;
                                 using BaSL.Executables.Attributes;
                                     
                                 namespace BaSL.CoreUtils;

                                 public sealed partial class Ls : App
                                 {

                                     [Execute]
                                     public async Task<int> MogusAsync([Flag('l')] bool longFormat)
                                     {
                                         return 0;
                                     }

                                 }
                                 """;

    public const string Result = """
                                 namespace BaSL.CoreUtils
                                 {
                                     partial class Ls
                                     {
                                     
                                     }
                                 }
                                 """;

}
