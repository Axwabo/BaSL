using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.Executables.Attributes;

namespace BaSL.BuiltIns;

using Result = Result<int, Error>;

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

    private const string Operators = "+-*/%";

    private static readonly Error DivideByZero = new DivideByZeroError();

    private static Result Evaluate(int x, char @operator, int y)
    {
        Result result = @operator switch
        {
            '+' => x + y,
            '-' => x - y,
            '*' => x * y,
            '/' when y == 0 => DivideByZero,
            '/' => x / y,
            '%' => x % y,
            _ => 0
        };
        return result;
    }

    private string _arg = "";

    private int _lastResult;

    public Let(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        foreach (var arg in Args)
        {
            if (string.IsNullOrWhiteSpace(arg))
                continue;
            _arg = arg;
            var equals = arg.IndexOf('=');
            // TODO: proper variable name validation
            if (arg[0] is not (>= 'A' and <= 'Z' or >= 'a' and <= 'z'))
                continue;
            if (equals < 1)
                await SingleOperand(cancellationToken);
            else
                await Process(equals, cancellationToken);
        }

        return _lastResult == 0 ? 1 : 0;
    }

    private async Task Process(int equals, CancellationToken cancellationToken)
    {
        var name = _arg.AsSpan(0, equals);
        var value = _arg.AsSpan(equals + 1).Trim();
        var index = value.IndexOfAny(Operators);
        if (Operators.Contains(name[^1]))
        {
            // TODO: evaluate left-hand expression
            if (int.TryParse(value, out var selfY))
                await EvaluateSelf(name[^1], selfY, name[..^1].ToString(), cancellationToken);
            return;
        }

        if (index == -1)
        {
            if (int.TryParse(value.Trim(), out var constant))
                Store(name.ToString(), constant);
            return;
        }

        if (index == value.Length - 1)
            return;
        int.TryParse(value[..index].Trim(), out var x);
        int.TryParse(value[(index + 1)..].Trim(), out var y);
        await Evaluate(x, value[index], y, name.ToString(), cancellationToken);
    }

    private async Task Evaluate(int x, char @operator, int y, string name, CancellationToken cancellationToken)
    {
        var result = Evaluate(x, @operator, y);
        if (result.Success)
        {
            Store(name, result.Value);
            return;
        }

        await StandardError.WriteAsync(_arg, cancellationToken);
        await StandardError.WriteAsync(": ", cancellationToken);
        await StandardError.WriteLineAsync(result.Error, cancellationToken);
    }

    private async Task EvaluateSelf(char @operator, int y, string name, CancellationToken cancellationToken)
    {
        int.TryParse(Local.GetValueOrDefault(name, "0"), out var value);
        await Evaluate(value, @operator, y, name, cancellationToken);
    }

    private Task SingleOperand(CancellationToken cancellationToken) => _arg switch
    {
        [.. var name, '+', '+'] => EvaluateSelf('+', 1, name, cancellationToken),
        [.. var name, '-', '-'] => EvaluateSelf('-', 1, name, cancellationToken),
        _ => Task.CompletedTask
    };

    private void Store(string name, int result)
    {
        Local[name] = result.ToString();
        _lastResult = result;
    }

}

file sealed record DivideByZeroError() : Error("division by 0");
