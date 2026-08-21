using System;

namespace BaSL.Executables.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class ExecuteAttribute : Attribute;
