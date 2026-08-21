using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace BaSL.SourceGenerators;

public static class OptionParser
{

    private const string FlagAttribute = $"{Helpers.Attributes}.FlagAttribute";
    private const string DirAttribute = $"{Helpers.Attributes}.DefaultToCurrentAttribute";

    public static void ProcessParameter(IParameterSymbol symbol, List<Option> list, CancellationToken token)
    {
        var type = symbol.Type.ToString();
        if (type == Helpers.TokenType)
        {
            list.Add(new CancellationTokenOption(symbol.Name));
            return;
        }

        if (type == Helpers.RestType)
        {
            list.Add(new RestArgumentsOption(symbol.Name));
            return;
        }

        if (type == Helpers.DirectoryType)
        {
            var useWorkingDir = false;
            foreach (var attribute in symbol.GetAttributes())
                useWorkingDir |= attribute.AttributeClass?.ToString() == DirAttribute;
            list.Add(new DirectoryOption(symbol.Name, useWorkingDir));
            return;
        }

        foreach (var attribute in symbol.GetAttributes())
        {
            token.ThrowIfCancellationRequested();
            if (attribute.AttributeClass?.ToString() != FlagAttribute)
                continue;
            list.Add(new FlagOption(
                symbol.Name,
                attribute.ConstructorArguments.Length != 0 && attribute.ConstructorArguments[0].Value is char flagChar ? flagChar : symbol.Name[0],
                symbol.NullableAnnotation != NullableAnnotation.Annotated,
                symbol.HasExplicitDefaultValue ? symbol.ExplicitDefaultValue as bool? : null
            ));
            return;
        }

        list.Add(new PositionalOption(symbol.Name, type, symbol.HasExplicitDefaultValue ? symbol.ExplicitDefaultValue?.ToString() : null));
    }

}
