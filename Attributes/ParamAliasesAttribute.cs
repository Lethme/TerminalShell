using Terminal.Exceptions;

namespace Terminal.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ParamAliasesAttribute : Attribute
{
    public string ParamName { get; }
    public IEnumerable<string> ParamAliases { get; }

    public ParamAliasesAttribute(string paramName, params string[] paramAliases)
    {
        var duplicatedAliased = paramAliases
            .GroupBy(p => p)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        if (duplicatedAliased.Any())
            throw new AliasesDuplicationException(
                $"Multiple parameter aliases declaration\n" +
                $"{paramName}: {String.Join(" ", duplicatedAliased)}"
            );
            
        this.ParamName = paramName;
        this.ParamAliases = paramAliases;
    }

    public override string ToString()
    {
        return $"Param: {ParamName}\nAliases:\n{String.Join("\n", ParamAliases)}";
    }
}