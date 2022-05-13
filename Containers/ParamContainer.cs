using System.Collections;

namespace Terminal.Containers;

public class ParamContainer : IEnumerable<object>, IDisposable
{
    public string Name { get; }
    public IEnumerable<string> Aliases { get; }
    public object[] Values { get; }

    internal ParamContainer(string name, IEnumerable<string> aliases, params object[] commandParams)
    {
        this.Name = name;
        this.Aliases = aliases;
        this.Values = commandParams;
    }
    internal ParamContainer(string name, IEnumerable<string> aliases, IEnumerable<object> commandParams)
    {
        this.Name = name;
        this.Aliases = aliases;
        this.Values = commandParams.ToArray();
    }

    internal static ParamContainer Create(string name, IEnumerable<string> aliases, params object[] commandParams) =>
        new ParamContainer(name, aliases, commandParams);
    internal static ParamContainer Create(string name, IEnumerable<string> aliases, IEnumerable<object> commandParams) =>
        new ParamContainer(name, aliases, commandParams);

    public IEnumerator<object> GetEnumerator()
    {
        return Values.AsEnumerable().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Dispose()
    {
    }
}