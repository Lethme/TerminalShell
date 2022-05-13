using Terminal.Containers;

namespace Terminal.Commands;

internal class Command : IDisposable
{
    internal string Name { get; }
    internal ParamsCollection Params { get; }

    internal Command(string name, ParamsCollection paramsCollection)
    {
        this.Name = name;
        this.Params = paramsCollection;
    }
    internal Command(string name, IEnumerable<ParamContainer> paramsCollection)
    {
        this.Name = name;
        this.Params = ParamsCollection.Create(paramsCollection);
    }
    internal Command(string name, params ParamContainer[] paramsCollection)
    {
        this.Name = name;
        this.Params = ParamsCollection.Create(paramsCollection);
    }

    public override string ToString()
    {
        var str = String.Empty;
        str += $"Command: {Name}\n";
        foreach (var param in Params)
        {
            str += $"Param: {param.Name}\n";
            foreach (var value in param.Values)
            {
                str += $"    {value} ({value.GetType().Name})\n";
            }
        }

        return str;
    }

    internal static Command Create(string name, ParamsCollection paramsCollection)
        => new Command(name, paramsCollection);
    internal static Command Create(string name, IEnumerable<ParamContainer> paramsCollection)
        => new Command(name, paramsCollection);
    internal static Command Create(string name, params ParamContainer[] paramsCollection)
        => new Command(name, paramsCollection);

    public void Dispose()
    {
    }
}