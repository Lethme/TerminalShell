using System.Reflection;
using Terminal.Attributes;
using Terminal.Extensions;

namespace Terminal.Commands;

internal class CommandInfo
{
    internal CommandsContext Context { get; }
    internal string Name { get; }
    internal IEnumerable<string> Aliases { get; }
    internal IEnumerable<string> DescriptionStrings { get; }
    internal IEnumerable<ParamInfo> Params { get; }
    internal MethodInfo Method { get; }
    internal string ContextName => Context.Name;
    internal UserCommandInfo UserCommandInfo => UserCommandInfo.Create(this);
    internal bool HasRequiredParams => Params.Any(p => p.Required);
    
    internal bool HasDuplicatedParams => Params.Any() && Params
        .GroupBy(param => param.Name)
        .Any(group => group.Count() > 1);
    
    internal bool HasDuplicatedParamsAliases => Params.Any() && Params
        .Select(p => p.Aliases)
        .Aggregate((a, b) => a.Concat(b))
        .GroupBy(alias => alias)
        .Any(group => @group.Count() > 1);

    internal bool HasParamsConflicts => Params.Count() > 1 && Params
        .Any(param => Params.Where(p => p.Name != param.Name).Any(p => p.Is(param.Name)));

    internal IEnumerable<string> DuplicatedParams => GetDuplicatedParams();
    internal IEnumerable<string> ParamsWithDuplicatedAliases => GetParamsWithDuplicatedAliases();

    internal CommandInfo(
        CommandsContext context,
        string name,
        IEnumerable<string> aliases,
        IEnumerable<string> descriptionStrings,
        IEnumerable<ParamInfo> commandParams,
        MethodInfo method
    )
    {
        this.Context = context;
        this.Name = name;
        this.Aliases = aliases;
        this.DescriptionStrings = descriptionStrings;
        this.Params = commandParams;
        this.Method = method;
    }

    internal bool HasParam(string paramName)
    {
        return Params
            .Any(param =>
                String.Equals(param.Name, paramName, StringComparison.CurrentCultureIgnoreCase) ||
                param.Aliases.Any(alias => String.Equals(alias, paramName, StringComparison.CurrentCultureIgnoreCase))
            );
    }

    internal IEnumerable<string> GetDuplicatedParams()
    {
        return Params
            .GroupBy(param => param.Name)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key);
    }

    internal IEnumerable<string> GetParamsWithDuplicatedAliases()
    {
        return Params
            .Select(p => p.Aliases)
            .Aggregate((a, b) => a.Concat(b))
            .GroupBy(alias => alias)
            .Where(group => group.Count() > 1)
            .Select(group => GetParams(group.Key))
            .Aggregate((a, b) => a.Union(b))
            .Select(param => param.Name);
    }

    internal ParamInfo? GetParam(string paramName)
    {
        return Params
            .FirstOrDefault(param => 
                String.Equals(param.Name, paramName, StringComparison.CurrentCultureIgnoreCase) ||
                param.Aliases.Any(alias => String.Equals(alias, paramName, StringComparison.CurrentCultureIgnoreCase))
            );
    }

    private IEnumerable<ParamInfo> GetParams(string paramName)
    {
        return Params
            .Where(param =>
                param.Name == paramName ||
                param.Aliases.Any(alias => alias == paramName)
            );
    }

    internal IEnumerable<ParamInfo> GetRequiredParams()
    {
        return Params.Where(param => param.Required);
    }

    public bool Is(string name)
    {
        return Name == name ||
               Aliases.Any(alias => alias == name);
    }

    internal object? Invoke(params object?[]? parameters)
    {
        return Method.Invoke(Context, parameters);
    }

    public string ToCollapsedString()
    {
        var commandName = $"Command: {Name}\n";
        var commandAliases = $"Aliases: {(!Aliases.Any() ? "-" : String.Join(" ", Aliases))}\n";
        var commandParameters = $"Parameters: {(!Params.Any() ? "-" : String.Join(" ", Params.Select(param => $"{(param.Required ? $"{param.Name}*" : param.Name)}{(!param.Aliases.Any() ? "" : $"{{{String.Join(" ", param.Aliases)}}}")}{(!param.ParamsTypes.Any() ? "" : $"({String.Join(" ", param.ParamsTypes.Select(type => type.Name))})")}")))}";

        return commandName + commandAliases + commandParameters;
    }

    public override string ToString()
    {
        var commandName = $"Command: {Name}" + (Aliases.Any() ? "" : "\n");
        var commandAliases = Aliases.Any() ? $"\nAliases: {String.Join(" ", Aliases)}\n" : "";
        var commandDescription = DescriptionStrings.Any() ? $"\n{String.Join("\n", DescriptionStrings)}\n" : "";
        var commandParameters = !Params.Any() ? "" : "\n" + String.Join("\n", Params
            .Select(param =>
            {
                var paramName = $"Parameter: {param.Name}\n";
                var paramAliases = param.Aliases.Any() ? $"Aliases: {String.Join(" ", param.Aliases)}\n" : "";
                var paramRequired = $"Required: {(param.Required ? "Yes" : "No")}\n";
                var paramValuesTypes = param.ParamsTypes.Any() ? $"Values: {String.Join(" ", param.ParamsTypes.Select(t => t.Name))}\n": "";
                var paramDescription = param.DescriptionStrings.Any()
                    ? $"Description: {String.Join(" | ", param.DescriptionStrings)}\n"
                    : "";

                return paramName + paramAliases + paramRequired + paramValuesTypes + paramDescription;
            })
        ).Trim('\n');

        return (commandName + commandAliases + commandDescription + commandParameters).Trim('\n');
    }

    internal static CommandInfo Create(
        CommandsContext context,
        string name,
        IEnumerable<string> aliases,
        IEnumerable<string> descriptionStrings,
        IEnumerable<ParamInfo> commandParams,
        MethodInfo method
    ) => new CommandInfo(context, name, aliases, descriptionStrings, commandParams, method);
}