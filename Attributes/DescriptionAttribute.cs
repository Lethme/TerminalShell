using System;
using System.Linq;
using Terminal.Extensions;

namespace Terminal.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class DescriptionAttribute : Attribute
{
    public IEnumerable<string> DescriptionStrings { get; }

    public DescriptionAttribute(string description)
    {
        this.DescriptionStrings = description.Split("\n").Trim(' ');
    }

    public DescriptionAttribute(params string[] descriptionStrings)
    {
        this.DescriptionStrings = descriptionStrings;
    }

    public override string ToString() => String.Join("\n", DescriptionStrings);
}