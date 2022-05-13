using Terminal.Attributes;
using Terminal.Containers;
using Terminal.Enums;
using Terminal.Extensions;
using Terminal.Core;

namespace Terminal.Commands;

[DefaultCommandsContext]
internal class DefaultCommandsContext : CommandsContext
{
    [Command(DefaultCommands.Help)]
    [Description("Used to see all the available commands", "You can also use 'help <command name>' to see any command's details")]
    [Param("command", typeof(string))]
    [ParamAliases("command", "cmd", "c")]
    [ParamDescription("command", "Any command's name")]
    public void Help(IEnumerable<CommandsContext> contexts, ParamsCollection paramsCollection)
    {
        var cmdName = paramsCollection.Get<string>("c", 0);
        
        if (cmdName != default)
        {
            var cmd = contexts.GetCommand(cmdName);
            
            if (cmd != default) LogLine(cmd);
            else LogLine($"Unknown command: {cmdName}");
        }
        else
        {
            foreach (var cmd in contexts.GetCommands())
            {
                var name = $"Command: {cmd.Name}";
                var aliases = cmd.Aliases.Any() ? $"Aliases: {String.Join(" ", cmd.Aliases)}" : "";
                var description = $"{String.Join("\n", cmd.DescriptionStrings)}";
                
                LogLine(name, aliases, description);
            }
        }
    }

    [Command(DefaultCommands.Commands)]
    [Aliases("cmds")]
    [Description("Used to see short commands description", "You can also use 'commands <command name>' to see the exact command's details")]
    [Param("command", typeof(string))]
    [ParamAliases("command", "cmd", "c")]
    [ParamDescription("command", "Any command's name")]
    public void Commands(IEnumerable<CommandsContext> contexts, ParamsCollection paramsCollection)
    {
        var cmdName = paramsCollection.Get<string>("c", 0);

        if (cmdName != default)
        {
            var cmd = contexts.GetCommand(cmdName);
            if (cmd != default) LogLine(cmd.ToCollapsedString());
            else LogLine($"Unknown command: {cmdName}");
        }
        else
        {
            foreach (var cmd in contexts.GetCommands())
            {
                LogLine(cmd.ToCollapsedString());
            }
        }
    }

    [Command(DefaultCommands.Clear)]
    [Aliases("clr")]
    [Description("Used to clear all current output")]
    public void Clear(ShellBase shell)
    {
        shell.Clear();
    }
    
    [Command(DefaultCommands.Exit)]
    [Aliases("e")]
    [Description("Used to exit from current shell")]
    public void Exit(ShellBase shell)
    {
        shell.Exit();
    }
}