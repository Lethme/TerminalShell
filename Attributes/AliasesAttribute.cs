using System;
using System.Linq;
using Terminal.Extensions;

namespace Terminal.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class AliasesAttribute : Attribute
{
    public IEnumerable<string> Names { get; }

    public AliasesAttribute(params string[] names)
    {
        if (names.Length != names.Distinct().Count())
            throw new ArgumentException("Aliases were duplicated");
        
        this.Names = names.ToLower();
    }

    public override string ToString() => String.Join(" ", Names);
}