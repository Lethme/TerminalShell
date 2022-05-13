namespace Terminal.Commands;

internal class ParamInfo
{
    internal string Name { get; }
    internal bool Required { get; }
    internal IEnumerable<string> Aliases { get; }
    internal IEnumerable<Type> ParamsTypes { get; }
    internal IEnumerable<string> DescriptionStrings { get; }
    internal UserParamInfo UserParamInfo => UserParamInfo.Create(this);

    internal ParamInfo(string name, bool required, IEnumerable<string> aliases, IEnumerable<string> descriptionStrings, IEnumerable<Type> paramsTypes)
    {
        this.Name = name;
        this.Aliases = aliases;
        this.DescriptionStrings = descriptionStrings;
        this.Required = required;
        this.ParamsTypes = paramsTypes;
    }

    internal ParamInfo(string name, bool required, IEnumerable<string> aliases, IEnumerable<string> descriptionStrings, params Type[] paramsTypes)
    {
        this.Name = name;
        this.Aliases = aliases;
        this.DescriptionStrings = descriptionStrings;
        this.Required = required;
        this.ParamsTypes = paramsTypes;
    }

    public bool Is(string key)
    {
        return String.Equals(Name, key, StringComparison.CurrentCultureIgnoreCase) ||
               Aliases.Any(alias => String.Equals(alias, key, StringComparison.CurrentCultureIgnoreCase));
    }
    
    public override string ToString()
    {
        return $"Param name: {Name}\n" +
               $"Param aliases: {String.Join(" ", Aliases)}\n" +
               $"Required: {Required}\n" +
               $"Description: {String.Join(" | ", DescriptionStrings)}\n" +
               $"Values: {String.Join(" ", ParamsTypes.Select(t => t.Name))}";
    }
    
    internal static ParamInfo Create(string name, bool required, IEnumerable<string> aliases, IEnumerable<string> descriptionStrings, IEnumerable<Type> paramsTypes) => new ParamInfo(name, required, aliases, descriptionStrings, paramsTypes);
    internal static ParamInfo Create(string name, bool required, IEnumerable<string> aliases, IEnumerable<string> descriptionStrings, params Type[] paramsTypes) => new ParamInfo(name, required, aliases, descriptionStrings, paramsTypes);
}