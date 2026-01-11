using System;

namespace BACApp.Core.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class CsvIgnoreAttribute : Attribute
{
}
