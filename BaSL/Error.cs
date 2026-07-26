using System;
using System.Diagnostics.CodeAnalysis;

namespace BaSL;

public abstract record Error(string Message);

public sealed class ErrorException : Exception
{

    public ErrorException(Error error) : base(error.Message) => Error = error;

    public Error Error { get; }

}

public static class ErrorExtensions
{

    extension(Error error)
    {

        [DoesNotReturn]
        public void Throw() => throw new ErrorException(error);

    }

}
