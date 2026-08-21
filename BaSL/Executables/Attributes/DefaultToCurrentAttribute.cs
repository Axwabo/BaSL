using System;

namespace BaSL.Executables.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class DefaultToCurrentAttribute : Attribute;
