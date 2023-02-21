using System.Diagnostics;
using System.Reflection;
using Terminal.Attributes;
using Terminal.Exceptions;
using Terminal.Extensions;
using Terminal.Core;

namespace Terminal.Commands;

public abstract class CommandsContext : IDisposable
{
    internal ShellBase shell;
    protected UserCommandInfo Command => GetCurrentCommand().UserCommandInfo;
    protected Shell Shell => (Shell)shell;
    internal string Name => GetType().Name;
    internal bool HasNonCommandMethods => GetAllActualMethods().Any(method => !method.HasAttribute<CommandAttribute>());
    internal bool HasPrivateCommandMethods => GetAllActualMethods().Any(method => method.HasAttribute<CommandAttribute>() && !method.IsPublic);
    internal bool HasAnyNonDefaultConstructor => GetAllActualConstructors().Any(constructor => constructor.GetParameters().Length != 0);
    internal bool HasDefaultConstructor => DefaultConstructor != default;
    internal bool IsDefaultConstructorPublic => DefaultConstructor != default && DefaultConstructor.IsPublic;
    internal bool HasFields => GetAllActualFields().Any();
    internal bool HasProperties => GetAllActualProperties().Any();
    internal ConstructorInfo? DefaultConstructor => GetDefaultConstructor();

    protected void Log(object? obj = null)
    {
        if (obj == null || obj.ToString() == String.Empty)
            return;
        
        Console.WriteLine(obj);
    }
    
    protected void Log(params object?[] objects)
    {
        objects.ForEach(obj =>
        {
            if (obj != null && obj.ToString() != String.Empty)
                Console.WriteLine(obj);
        });
    }
    
    protected void LogLine(object? obj = null)
    {
        if (obj == null)
        {
            Console.WriteLine();
            return;
        }

        if (obj.ToString() == String.Empty)
        {
            return;
        }
        
        Console.WriteLine($"{obj}\n");
    }
    
    protected void LogLine(params object?[] objects)
    {
        if (objects.Length == 0)
        {
            Console.WriteLine();
            return;
        }
        
        objects.ForEach(obj =>
        {
            if (obj != null && obj.ToString() != String.Empty)
                Console.WriteLine(obj);
        });
        
        Console.WriteLine();
    }
    
    private CommandInfo GetCurrentCommand()
    {
        var stackTrace = new StackTrace();

        var commandMethods = GetCommandMethods();
        var method = stackTrace
            .GetFrames()
            .Select(frame => frame.GetMethod())
            .FirstOrDefault(method => commandMethods.Contains(method));

        if (method == default)
            throw new CommandsContextException(
                $"Tried to get command context inside a non-command method"
            );

        return GetCommandByMethod(method)!;
    }

    private CommandInfo? GetCommandByMethod(MethodBase method)
    {
        return GetCommands()
            .FirstOrDefault(cmd => cmd.Method.Equals(method));
    }

    internal IEnumerable<MethodInfo> GetMethods()
    {
        return this.GetType().GetMethods();
    }

    internal IEnumerable<MethodInfo> GetMethods(BindingFlags bindingAttr)
    {
        return this.GetType().GetMethods(bindingAttr);
    }

    internal IEnumerable<MethodInfo> GetActualMethods()
    {
        var methods = typeof(CommandsContext).GetMethods().Select(method => method.Name);
        return this.GetType().GetMethods().Where(method => !methods.Contains(method.Name));
    }
    
    internal IEnumerable<MethodInfo> GetAllActualMethods()
    {
        var methods = typeof(CommandsContext)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
            .Select(method => method.Name);
        
        return this
            .GetType()
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
            .Where(method => !methods.Contains(method.Name));
    }

    internal IEnumerable<ConstructorInfo> GetAllActualConstructors()
    {
        return this
            .GetType()
            .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
    }

    internal IEnumerable<FieldInfo> GetAllFields()
    {
        return GetType()
            .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
    }

    internal IEnumerable<PropertyInfo> GetAllProperties()
    {
        return GetType()
            .GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
    }
    
    internal IEnumerable<FieldInfo> GetAllActualFields()
    {
        var fields = typeof(CommandsContext)
            .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .Select(field => field.Name);
        
        return GetType()
            .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .Where(field => !fields.Contains(field.Name));
    }

    internal IEnumerable<PropertyInfo> GetAllActualProperties()
    {
        var properties = typeof(CommandsContext)
            .GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .Select(property => property.Name);
        
        return GetType()
            .GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .Where(property => !properties.Contains(property.Name));
    }

    internal IEnumerable<MethodInfo> GetNonCommandMethods()
    {
        return GetAllActualMethods()
            .Where(method => !method.HasAttribute<CommandAttribute>());
    }

    internal IEnumerable<MethodInfo> GetPrivateCommandMethods()
    {
        return GetAllActualMethods()
            .Where(method => method.HasAttribute<CommandAttribute>() && !method.IsPublic);
    }

    internal IEnumerable<MethodInfo> GetMethods<TAttribute>()
        where TAttribute : Attribute
    {
        return this.GetMethods()
            .Where(method => method.HasAttribute<TAttribute>());
    }

    internal IEnumerable<MethodInfo> GetCommandMethods()
    {
        return this.GetMethods()
            .Where(method => method.HasAttribute<CommandAttribute>());
    }

    internal IEnumerable<CommandInfo> GetCommands()
    {
        var methods = GetMethods<CommandAttribute>();

        return !methods.Any()
            ? Array.Empty<CommandInfo>()
            : methods
                .Select(method => CommandInfo.Create
                (
                    this,
                    method.GetCommandName()!,
                    method.GetCommandAliases(),
                    method.GetCommandDescription(),
                    method.GetCommandParams(),
                    method
                ));
    }

    internal ConstructorInfo? GetDefaultConstructor()
    {
        return GetAllActualConstructors()
            .FirstOrDefault(constructor => constructor.GetParameters().Length == 0);
    }

    internal static IEnumerable<CommandsContext> Contexts => GetContexts();
    internal static IEnumerable<Type> ContextsTypes => GetContextsTypes();
    internal static IEnumerable<CommandsContext> GetContexts()
    {
        var classes = Assembly
            .GetEntryAssembly()!
            .ExportedTypes
            .Where(type => typeof(CommandsContext).IsAssignableFrom(type) && typeof(CommandsContext) != type && !type.IsAbstract);

        if (!classes.Any())
            return Array.Empty<CommandsContext>();

        var contexts = classes
            .Select(c => (CommandsContext)Activator.CreateInstance(c)!);

        return contexts;
    }
    
    internal static IEnumerable<Type> GetContextsTypes()
    {
        var classes = Assembly
            .GetEntryAssembly()!
            .ExportedTypes
            .Where(type => typeof(CommandsContext).IsAssignableFrom(type) && typeof(CommandsContext) != type && !type.IsAbstract);

        if (!classes.Any())
            return Array.Empty<Type>();

        return classes;
    }

    public void Dispose()
    {
        
    }
}