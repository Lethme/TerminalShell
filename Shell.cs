using Terminal.Commands;
using Terminal.Core;

namespace Terminal;

public class Shell : ShellBase
{
    internal Shell(
        string name,
        string commandMarker = DefaultCommandMarker,
        string paramsMarker = DefaultParamsMarker
    ) : base(name, commandMarker, paramsMarker) { }
    internal Shell(
        IEnumerable<Type> contexts,
        string name, 
        string commandMarker = DefaultCommandMarker,
        string paramsMarker = DefaultParamsMarker
    ) : base(contexts, name, commandMarker, paramsMarker) { }

    public static Shell Create(
        string name,
        string commandMarker = DefaultCommandMarker,
        string paramsMarker = DefaultParamsMarker
    ) => new Shell(name, commandMarker, paramsMarker);
    
    public static ShellOptions UseBuilder() => new();
}