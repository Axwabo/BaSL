using System;

namespace BaSL.Executables;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class ExecuteAttribute : Attribute;
