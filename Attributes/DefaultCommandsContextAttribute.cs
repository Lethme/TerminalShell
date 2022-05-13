using System.Reflection;
using Terminal.Commands;
using Terminal.Enums;
using Terminal.Exceptions;

namespace Terminal.Attributes;

[AttributeUsage(AttributeTargets.Class)]
internal class DefaultCommandsContextAttribute : Attribute
{
    public DefaultCommandsContextAttribute()
    {
        
    }
}