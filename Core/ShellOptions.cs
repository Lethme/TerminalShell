using Terminal.Commands;
using Terminal.Exceptions;
using Terminal.Extensions;

namespace Terminal.Core;

public class ShellOptions : IDisposable
{
    private List<Type> CommandsContexts { get; }

    internal ShellOptions()
    {
        CommandsContexts = new List<Type>();
    }
    
    public ShellOptions AddCommandsContext<TCommandsContext>()
        where TCommandsContext : CommandsContext, new()
    {
        CommandsContexts.Add(typeof(TCommandsContext));
        return this;
    }

    public Shell BuildShell(string name, string commandMarker = ShellBase.DefaultCommandMarker, string paramsMarker = ShellBase.DefaultParamsMarker)
    {
        return !CommandsContexts.Any()
            ? new Shell(name, commandMarker, paramsMarker) 
            : new Shell(CommandsContexts, name, commandMarker, paramsMarker);
    }

    public void Dispose()
    {
        
    }
}