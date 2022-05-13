using System.Collections;
using System.Reflection;
using Terminal.Attributes;
using Terminal.Commands;
using Terminal.Containers;

namespace Terminal.Extensions;

public static class IEnumerableExtensions
{
    public static void Log(this IEnumerable<object?> src)
    {
        src.ForEach(obj =>
        {
            if (obj != null && obj.ToString() != String.Empty)
                Console.WriteLine(obj);
        });
    }

    public static void LogLine(this IEnumerable<object?> src)
    {
        src.ForEach(obj =>
        {
            if (obj != null && obj.ToString() != String.Empty)
                Console.WriteLine(obj);
        });

        Console.WriteLine();
    }

    public static IEnumerable<T> Sort<T>(this IEnumerable<T> src)
    {
        var tempArray = src.ToArray();
        Array.Sort(tempArray);

        return tempArray.AsEnumerable();
    }

    public static IEnumerable<T> Sort<T>(this IEnumerable<T> src, Comparison<T> comparison)
    {
        var tempArray = src.ToArray();
        Array.Sort(tempArray, comparison);

        return tempArray.AsEnumerable();
    }

    public static void ForEach<T>(this IEnumerable<T> src, Action<T> action)
    {
        foreach (var item in src)
        {
            action.Invoke(item);
        }
    }

    public static void ForEach<T>(this IEnumerable<T> src, Action<T, int> action)
    {
        var index = 0;
        foreach (var item in src)
        {
            action.Invoke(item, index++);
        }
    }

    public static void Enumerate<T>(this IEnumerable<T> src, Action<T, T> action)
    {
        var enumerator = src.GetEnumerator();
        enumerator.MoveNext();
        var current = enumerator.Current;
        while (enumerator.MoveNext())
        {
            action.Invoke(current, enumerator.Current);
            current = enumerator.Current;
        }
    }

    internal static bool ContainsShellArray(this IEnumerable<Type> src)
    {
        return src.Any(type => type.IsShellArray());
    }
    
    internal static IEnumerable<string> Trim(this IEnumerable<string> src, char symbol)
    {
        return src.Select(str => str.Trim(symbol));
    }

    internal static IEnumerable<string> ToLower(this IEnumerable<string> src)
    {
        return src.Select(s => s.ToLower());
    }

    internal static IEnumerable<CommandsContext> GetCustomCommandsContexts(this IEnumerable<CommandsContext> contexts)
    {
        if (!contexts.Any())
            return Array.Empty<CommandsContext>();

        return contexts
            .Where(ctx => !ctx.HasAttribute<DefaultCommandsContextAttribute>());
    }

    internal static IEnumerable<MethodInfo> GetCommandMethods(this IEnumerable<CommandsContext> contexts)
    {
        if (!contexts.Any())
            return Array.Empty<MethodInfo>();

        return contexts
            .Select(ctx => ctx.GetCommandMethods())
            .Aggregate((a, b) => a.Concat(b));
    }

    internal static bool HasNonCommandMethods(this IEnumerable<CommandsContext> contexts)
    {
        return contexts
            .Any(ctx => ctx.HasNonCommandMethods);
    }

    internal static IEnumerable<(string ContextName, IEnumerable<string> Methods)> GetNonCommandMethods(
        this IEnumerable<CommandsContext> contexts)
    {
        return contexts
            .Where(ctx => ctx.HasNonCommandMethods)
            .Select(ctx => (ctx.Name, ctx.GetNonCommandMethods().Select(method => method.Name)));
    }

    internal static bool HasPrivateCommandMethods(this IEnumerable<CommandsContext> contexts)
    {
        return contexts
            .Any(ctx => ctx.HasPrivateCommandMethods);
    }

    internal static IEnumerable<(string ContextName, IEnumerable<string> Methods)> GetPrivateCommandMethods(
        this IEnumerable<CommandsContext> contexts)
    {
        return contexts
            .Where(ctx => ctx.HasPrivateCommandMethods)
            .Select(ctx => (ctx.Name, ctx.GetPrivateCommandMethods().Select(method => method.Name)));
    }

    internal static bool HasFields(this IEnumerable<CommandsContext> contexts)
    {
        return contexts
            .Any(ctx => ctx.HasFields);
    }

    internal static IEnumerable<(string ContextName, IEnumerable<string> Fields)> GetFields(
        this IEnumerable<CommandsContext> contexts)
    {
        return contexts
            .Where(ctx => ctx.HasFields)
            .Select(ctx => (ctx.Name, ctx.GetAllActualFields().Select(field => field.Name)));
    }

    internal static bool HasProperties(this IEnumerable<CommandsContext> contexts)
    {
        return contexts
            .Any(ctx => ctx.HasProperties);
    }

    internal static IEnumerable<(string ContextName, IEnumerable<string> Properties)> GetProperties(
        this IEnumerable<CommandsContext> contexts)
    {
        return contexts
            .Where(ctx => ctx.HasProperties)
            .Select(ctx => (ctx.Name, ctx.GetAllActualProperties().Select(property => property.Name)));
    }

    internal static bool IsCommandMethodsValid(this IEnumerable<CommandsContext> contexts)
    {
        if (!contexts.Any())
            return false;

        var methods = contexts.GetCustomCommandsContexts().GetCommandMethods();

        if (!methods.Any())
            return true;

        return !methods.Any(method => !method.IsValidCommandMethod());
    }

    internal static IEnumerable<CommandInfo> GetInvalidCommands(this IEnumerable<CommandsContext> contexts)
    {
        if (!contexts.Any())
            return Array.Empty<CommandInfo>();

        var commands = contexts.GetCustomCommandsContexts().GetCommands();
        var methods = commands
            .Select(cmd => cmd.Method);

        if (!methods.Any())
            return Array.Empty<CommandInfo>();

        // return methods
        //     .Where(method => !method.IsValidCommandMethod());
        return commands
            .Where(cmd => !cmd.Method.IsValidCommandMethod());
    }

    internal static IEnumerable<CommandInfo> GetCommands(this IEnumerable<CommandsContext> contexts)
    {
        if (!contexts.Any())
            return Array.Empty<CommandInfo>();

        return contexts
            .Select(context => context.GetCommands())
            .Aggregate((a, b) => a.Concat(b));
    }

    internal static IEnumerable<CommandInfo> GetCommands(this IEnumerable<CommandsContext> contexts, string alias)
    {
        if (!contexts.Any())
            return Array.Empty<CommandInfo>();

        return contexts
            .Select(context => context.GetCommands())
            .Aggregate((a, b) => a.Concat(b))
            .Where(command => command.Is(alias));
    }

    internal static CommandInfo? GetCommand(this IEnumerable<CommandsContext> contexts, string commandName)
    {
        return contexts
            .GetCommands()
            .FirstOrDefault(cmd => cmd.Is(commandName));
    }

    internal static IEnumerable<CommandInfo> GetDefaultCommands(this IEnumerable<CommandsContext> contexts)
    {
        if (!contexts.Any())
            return Array.Empty<CommandInfo>();

        return contexts
            .Where(context => context.HasAttribute<DefaultCommandsContextAttribute>())
            .Select(context => context.GetCommands())
            .Aggregate((a, b) => a.Concat(b));
    }

    internal static IEnumerable<CommandInfo> GetUserCommands(this IEnumerable<CommandsContext> contexts)
    {
        if (!contexts.Any(ctx => !ctx.HasAttribute<DefaultCommandsContextAttribute>()))
            return Array.Empty<CommandInfo>();

        return contexts
            .Where(context => !context.HasAttribute<DefaultCommandsContextAttribute>())
            .Select(context => context.GetCommands())
            .Aggregate((a, b) => a.Concat(b));
    }

    internal static bool HasDuplicatedMethods(this IEnumerable<CommandsContext> contexts)
    {
        return contexts
            .GetCommands()
            .GroupBy(cmd => cmd.Method.Name)
            .Any(group => group.Count() > 1);
    }

    internal static IEnumerable<(string MethodName, IEnumerable<string> Contexts)> GetDuplicatedMethods(
        this IEnumerable<CommandsContext> contexts)
    {
        return contexts
            .GetCommands()
            .GroupBy(cmd => cmd.Method.Name)
            .Where(group => group.Count() > 1)
            .Select(group => (group.Key, group.Select(cmd => cmd.ContextName)));
    }

    internal static bool HasParamsConflicts(this IEnumerable<CommandsContext> contexts)
    {
        return contexts
            .GetUserCommands()
            .Any(cmd => cmd.HasParamsConflicts);
    }

    internal static IEnumerable<string> GetCommandsWithConflictingParams(this IEnumerable<CommandsContext> contexts)
    {
        return contexts
            .GetUserCommands()
            .Where(cmd => cmd.HasParamsConflicts)
            .Select(cmd => cmd.Name);
    }

    internal static bool HasDuplicatedCommands(this IEnumerable<CommandsContext> contexts)
    {
        var commands = contexts.GetCommands();

        if (!commands.Any())
            return false;

        return commands.Count() != commands.DistinctBy(command => command.Name).Count();
    }

    internal static IEnumerable<string> GetDuplicatedCommands(this IEnumerable<CommandsContext> contexts)
    {
        return contexts
            .GetCommands()
            .GroupBy(command => command.Name)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key);
    }

    internal static IEnumerable<string> GetDuplicatedCommands(this IEnumerable<CommandsContext> contexts,
        IEnumerable<string> aliases)
    {
        return contexts
            .GetCommands()
            .GroupBy(command => command.Name)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key);
    }

    internal static bool HasDuplicatedAliases(this IEnumerable<CommandsContext> contexts)
    {
        var commands = contexts.GetCommands();

        if (!commands.Any())
            return false;

        var aliases = commands
            .Select(command => command.Aliases)
            .Aggregate((a, b) => a.Concat(b))
            .ToArray();

        return aliases.Length != aliases.Distinct().Count();
    }

    internal static IEnumerable<string> GetDuplicatedAliases(this IEnumerable<CommandsContext> contexts)
    {
        var aliases = contexts
            .GetCommands()
            .Select(command => command.Aliases)
            .Aggregate((a, b) => a.Concat(b));

        return aliases
            .GroupBy(alias => alias)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key);
    }

    internal static bool HasDuplicatedParams(this IEnumerable<CommandsContext> contexts)
    {
        return contexts
            .GetUserCommands()
            .Any(cmd => cmd.HasDuplicatedParams);
    }

    internal static IEnumerable<(string CommandName, IEnumerable<string> DuplicatedParams)> GetDuplicatedParams(
        this IEnumerable<CommandsContext> contexts)
    {
        return contexts
            .GetUserCommands()
            .Where(cmd => cmd.HasDuplicatedParams)
            .Select(cmd => (cmd.Name, cmd.DuplicatedParams));
    }

    internal static bool HasParamsWithDuplicatedAliases(this IEnumerable<CommandsContext> contexts)
    {
        return contexts
            .GetUserCommands()
            .Any(cmd => cmd.HasDuplicatedParamsAliases);
    }

    internal static IEnumerable<(string CommandName, IEnumerable<string> DuplicatedParams)>
        GetParamsWithDuplicatedAliases(this IEnumerable<CommandsContext> contexts)
    {
        return contexts
            .GetUserCommands()
            .Where(cmd => cmd.HasDuplicatedParamsAliases)
            .Select(cmd => (cmd.Name, cmd.ParamsWithDuplicatedAliases));
    }

    internal static bool HasEmptyContext(this IEnumerable<CommandsContext> contexts)
    {
        return contexts
            .Select(ctx => ctx.GetCommands())
            .Any(commands => !commands.Any());
    }

    internal static IEnumerable<CommandsContext> GetEmptyContexts(this IEnumerable<CommandsContext> contexts)
    {
        return contexts
            .Where(ctx => !ctx.GetCommands().Any());
    }

    internal static bool HasNonDefaultConstructors(this IEnumerable<Type> contexts)
    {
        return contexts
            .Any(ctx => ctx.HasNonDefaultConstructors());
    }
    
    internal static IEnumerable<string> GetNonDefaultConstructorsContexts(this IEnumerable<Type> contexts)
    {
        return contexts
            .Where(ctx => ctx.HasNonDefaultConstructors())
            .Select(ctx => ctx.Name);
    }
    
    internal static IEnumerable<string> GetNonDefaultConstructorsContexts(this IEnumerable<CommandsContext> contexts)
    {
        return contexts
            .Where(context => context.HasAnyNonDefaultConstructor)
            .Select(context => context.Name);
    }

    internal static bool IsDefaultConstructorsPublic(this IEnumerable<Type> contexts)
    {
        return contexts
            .All(ctx => ctx.IsDefaultConstructorPublic());
    }
    
    internal static IEnumerable<string> GetNonPublicDefaultConstructorsContexts(this IEnumerable<Type> contexts)
    {
        return contexts
            .Where(ctx => !ctx.IsDefaultConstructorPublic())
            .Select(ctx => ctx.Name);
    }

    internal static bool HasDefaultConstructors(this IEnumerable<Type> contexts)
    {
        return contexts
            .All(ctx => ctx.HasDefaultConstructor());
    }
    
    internal static IEnumerable<string> GetNoDefaultConstructorsContexts(this IEnumerable<Type> contexts)
    {
        return contexts
            .Where(ctx => !ctx.HasDefaultConstructor())
            .Select(ctx => ctx.Name);
    }

    internal static bool HasCommand(this IEnumerable<CommandInfo> commands, string command)
    {
        return commands.Any(cmd =>
            cmd.Name == command.ToLower() ||
            cmd.Aliases.Any(alias => alias == command.ToLower())
        );
    }

    internal static IEnumerable<CommandsContext> CreateContexts(this IEnumerable<Type> contexts)
    {
        if (!contexts.Any())
            return Array.Empty<CommandsContext>();
        
        return contexts
            .Select(ctx => ctx.CreateInstance<CommandsContext>()!);
    }
}