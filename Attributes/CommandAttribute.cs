using System;

namespace Terminal.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute : Attribute
{
    public string Name { get; }

    public CommandAttribute(string name)
    {
        this.Name = name.ToLower();
    }

    public override string ToString() => this.Name;
}