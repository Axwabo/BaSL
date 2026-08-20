using System;
using BaSL.Executables;
using BaSL.FileSystems;
using BaSL.FileSystems.Errors;
using BaSL.Syntax;

namespace BaSL.Interpreter;

internal static class Conditions
{

    public static bool? IsTrueComplex(Args condition, Directory workingDirectory)
    {
        // TODO: -a
        foreach (var and in condition.Value.Split("&&"))
        {
            var @true = false;
            foreach (var or in and.Split("||"))
            {
                if (IsTrue(or, workingDirectory) is not { } value)
                    return null;
                @true |= value;
                if (value)
                    break;
            }

            if (!@true)
                return false;
        }

        return true;
    }

    private static bool IsTrue(string a, Operator @operator, string b) => @operator switch
    {
        Operator.Equals => a == b,
        Operator.NotEquals => a != b,
        Operator.LeftGreaterThanRight when double.TryParse(a, out var x) && double.TryParse(b, out var y) => x > y,
        Operator.LeftGreaterThanRight => a.CompareTo(b, StringComparison.CurrentCultureIgnoreCase) < 0,
        Operator.LeftLessThanRight when double.TryParse(a, out var x) && double.TryParse(b, out var y) => x < y,
        Operator.LeftLessThanRight => a.CompareTo(b, StringComparison.CurrentCultureIgnoreCase) > 0,
        _ => throw new NotImplementedException()
    };

    private static bool? IsTrue(string left, string op, string right, bool @true)
    {
        Operator? @operator = op switch
        {
            "=" or "==" or "-eq" => Operator.Equals,
            "!=" or "-ne" => Operator.NotEquals,
            "<" or "-lt" => Operator.LeftLessThanRight,
            ">" or "-gt" => Operator.LeftGreaterThanRight,
            _ => null
        };
        return @operator == null
            ? null
            : IsTrue(left, @operator.Value, right) == @true;
    }

    private static bool? IsTrue(Args args, Directory workingDirectory) => args switch
    {
        ["true" or "1"] => true,
        ["false" or "0"] => false,
        [var op, var str] => IsTrue(op, str, true, workingDirectory),
        ["!", var op, var str] => IsTrue(op, str, false, workingDirectory),
        [var left, var op, var right] => IsTrue(left, op, right, true),
        ["!", var left, var op, var right] => IsTrue(left, op, right, false),
        _ => null
    };

    private static bool? IsTrue(string op, string str, bool @true, Directory workingDirectory) => op switch
    {
        "-z" => string.IsNullOrEmpty(str) == @true,
        "-n" => !string.IsNullOrEmpty(str) == @true,
        "-e" => workingDirectory.GetEntry(str).Error is not NotFoundError == @true, // TODO: write error?
        "-f" => workingDirectory.GetEntry(str).Value is File == @true,
        "-d" => workingDirectory.GetEntry(str).Value is Directory == @true,
        "-h" or "-L" => workingDirectory.GetEntry(str).Value is SymbolicLink == @true,
        _ => null
    };

}
