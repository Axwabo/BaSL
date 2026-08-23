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
internal sealed partial class Let : BuiltInCommand
{

    private const string Operators = "+=*/%";

    private static readonly Error DivideByZero = new DivideByZeroError();

    private int _lastResult;

    public Let(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        foreach (var arg in Args)
        {
            var equals = arg.IndexOf('=');
            // TODO: proper variable name validation
            if (equals < 1 || arg[0] is not (>= 'A' and <= 'Z' or >= 'a' and <= 'z'))
                continue;
            var name = arg[..equals];
            switch (arg.AsSpan(equals + 1))
            {
                case [.. var name, '=']:
                    if (int.TryParse(value, out))
                        break;
            }
        }

        return _lastResult == 0 ? 1 : 0;
    }

    /*
    protected  async void Process(string name, string value, CancellationToken cancellationToken)
    {
        var span = value.AsSpan();
        var index = span.IndexOfAny(Operators);
        if (index == -1)
        {
            await SingleOperand(name, value, cancellationToken);
            return;
        }

        if (index == span.Length - 1)
            return;

        int.TryParse(span[..index].Trim(), out var x);
        int.TryParse(span[(index + 1)..].Trim(), out var y);
        await Evaluate(x, span[index], y, name, cancellationToken);
    }

    private async Task Evaluate(int x, char @operator, int y, string name, CancellationToken cancellationToken)
    {
        Result<int, Error> result = @operator switch
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

    private async Task EvaluateSelf(char @operator, int y, string name, CancellationToken cancellationToken)
    {
        int.TryParse(Local.GetValueOrDefault(name, "0"), out var value);
        await Evaluate(value, @operator, y, name, cancellationToken);
    }

    private async Task SingleOperand(string name, string value, CancellationToken cancellationToken)
    {
        /*var x = name switch
        {
            ['+', '+'] => "",
             _=>"a"
        };#1#
        if (value.EndsWith("++"))
            await EvaluateSelf('+', 1, name, cancellationToken);
        else if (int.TryParse(value.AsSpan().Trim(), out var single))
            Store(name, single);
    }

    private void Store(string name, int result)
    {
        Local[name] = result.ToString();
        _lastResult = result;
    }
    */

}

file sealed record DivideByZeroError() : Error("division by 0");
