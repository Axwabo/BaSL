using System;
using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.Executables.Attributes;

namespace BaSL.BuiltIns;

[Help("""
      Evaluates a simple integer arithmetic expression (constant or two operands), and sets a variable to that value.
      Exits with code 1 if the last argument evaluates to 0.
      Examples:
      let a=1+2
      let 'b = 5 * 10'
      let sus=4+20 "c = $b * 20"
      """)]
internal sealed partial class Let : VariableCommand
{

    private static readonly Error DivideByZero = new DivideByZeroError();

    public Let(ExecutableContext context) : base(context)
    {
    }

    protected override async Task Process(string name, string value, CancellationToken cancellationToken)
    {
        var span = value.AsSpan();
        var index = span.IndexOfAny("+=*/%");
        if (index == -1)
        {
            if (int.TryParse(span.Trim(), out var single))
                Store(name, single);
            return;
        }

        int.TryParse(span[..index].Trim(), out var x);
        int.TryParse(span[(index + 1)..].Trim(), out var y);
        Result<int, Error> result = span[index] switch
        {
            '+' => x + y,
            '-' => x - y,
            '*' => x * y,
            '/' when y == 0 => DivideByZero,
            '/' => x / y,
            '%' => x % y,
            _ => 0
        };
        if (result.Success)
        {
            Store(name, result.Value);
            return;
        }

        await StandardError.WriteAsync(Arg, cancellationToken);
        await StandardError.WriteAsync(": ", cancellationToken);
        await StandardError.WriteLineAsync(result.Error, cancellationToken);
    }

    private void Store(string name, int result)
    {
        Local[name] = result.ToString();
        ExitCode = result == 0 ? 1 : 0;
    }

}

file sealed record DivideByZeroError() : Error("division by 0");
