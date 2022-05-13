using System.Reflection;
using Terminal.Attributes;
using Terminal.Commands;
using Terminal.Containers;

namespace Terminal.Extensions;

internal static class MethodInfoExtensions
{
    internal static IEnumerable<TAttribute> GetAttributes<TAttribute>(this MethodInfo method)
        where TAttribute : Attribute
    {
        return method
            .GetCustomAttributes(typeof(TAttribute))
            .Select(attr => (TAttribute)Convert.ChangeType(attr, typeof(TAttribute)));
    }

    internal static bool HasAttribute<TAttribute>(this MethodInfo method)
        where TAttribute : Attribute
    {
        return method.GetCustomAttributes(typeof(TAttribute)).Any();
    }

    internal static IEnumerable<string> GetCommandDescription(this MethodInfo method)
    {
        if (!method.HasAttribute<DescriptionAttribute>())
            return Array.Empty<string>();

        return method
            .GetAttributes<DescriptionAttribute>()
            .First()
            .DescriptionStrings;
    }

    internal static string? GetCommandName(this MethodInfo method)
    {
        if (!method.HasAttribute<CommandAttribute>())
            return null;

        return method
            .GetAttributes<CommandAttribute>()
            .First()
            .Name;
    }
    internal static IEnumerable<string> GetCommandAliases(this MethodInfo method)
    {
        if (!(method.HasAttribute<CommandAttribute>() && method.HasAttribute<AliasesAttribute>()))
            return Array.Empty<string>();

        return method
            .GetAttributes<AliasesAttribute>()
            .First()
            .Names;
    }
    internal static IEnumerable<ParamInfo> GetCommandParams(this MethodInfo method)
    {
        if (!(method.HasAttribute<CommandAttribute>() && method.HasAttribute<ParamAttribute>()))
            return Array.Empty<ParamInfo>();

        return method
            .GetAttributes<ParamAttribute>()
            .Select(attr =>
            {
                var paramDescriptionAttr = method
                    .GetAttributes<ParamDescriptionAttribute>()
                    .FirstOrDefault(a =>
                        String.Equals(a.ParamName, attr.Name, StringComparison.CurrentCultureIgnoreCase));
                
                var paramAliasesAttr = method
                    .GetAttributes<ParamAliasesAttribute>()
                    .FirstOrDefault(a => 
                        String.Equals(a.ParamName, attr.Name, StringComparison.CurrentCultureIgnoreCase));

                var description = paramDescriptionAttr != default
                    ? paramDescriptionAttr.DescriptionStrings
                    : Array.Empty<string>();

                var aliases = paramAliasesAttr != default
                    ? paramAliasesAttr.ParamAliases
                    : Array.Empty<string>();

                return ParamInfo.Create(attr.Name.ToLower(), attr.Required, aliases.ToLower(), description, attr.ParamsTypes);
            });
    }

    internal static bool IsValidCommandMethod(this MethodInfo method)
    {
        var hasParams = method.HasAttribute<ParamAttribute>()
                        && method.GetParameters().Length == 1
                        && method.GetParameters().First().ParameterType == typeof(ParamsCollection);

        var hasNoParams = !method.HasAttribute<ParamAttribute>()
                          && method.GetParameters().Length == 0;

        return hasParams || hasNoParams;
    }
}