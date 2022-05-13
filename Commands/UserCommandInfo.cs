namespace Terminal.Commands;

public class UserCommandInfo
{
    private CommandInfo Command { get; }
    public string Name => Command.Name;
    public IEnumerable<string> Aliases => Command.Aliases;
    public IEnumerable<string> DescriptionStrings => Command.DescriptionStrings;
    public IEnumerable<UserParamInfo> Params => Command.Params.Select(p => p.UserParamInfo);

    internal UserCommandInfo(CommandInfo command)
    {
        Command = command;
    }

    public override string ToString() => Command.ToString();

    internal static UserCommandInfo Create(CommandInfo command)
        => new UserCommandInfo(command);
}