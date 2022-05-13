using System;
using System.Linq;
using Terminal.Extensions;

namespace Terminal.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ParamDescriptionAttribute : Attribute
{
    public string ParamName { get; }
    public IEnumerable<string> DescriptionStrings { get; }

    public ParamDescriptionAttribute(string paramName, string description)
    {
        this.ParamName = paramName;
        this.DescriptionStrings = description.Split("\n").Trim(' ');
    }

    public ParamDescriptionAttribute(string paramName, params string[] descriptionStrings)
    {
        this.ParamName = paramName;
        this.DescriptionStrings = descriptionStrings;
    }

    public override string ToString() => String.Join("\n", DescriptionStrings);
}