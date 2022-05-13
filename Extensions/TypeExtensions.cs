using System.Reflection;
using Terminal.Core;

namespace Terminal.Extensions;

internal static class TypeExtensions
{
    internal static T? CreateInstance<T>(this Type type)
    {
        return (T)Activator.CreateInstance(type)!;
    }

    internal static bool IsShellArray(this Type type)
    {
        return type.IsArray &&
               ShellBase.AllowedTypes.Contains(type.GetElementType());
    }
    internal static bool IsDefaultConstructorPublic(this Type type)
    {
        return type.HasDefaultConstructor() && type.GetDefaultContructor()!.IsPublic;
    }

    internal static bool HasNonDefaultConstructors(this Type type)
    {
        return type
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
            .Any(constructor => constructor.GetParameters().Length != 0);
    }
    internal static bool HasDefaultConstructor(this Type type)
    {
        return type
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
            .Any(constructor => constructor.GetParameters().Length == 0);
    }
    internal static ConstructorInfo? GetDefaultContructor(this Type type)
    {
        return type
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
            .FirstOrDefault(contructor => contructor.GetParameters().Length == 0);
    }
}