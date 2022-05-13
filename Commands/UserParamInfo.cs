namespace Terminal.Commands;

public class UserParamInfo
{
    private ParamInfo Param { get; }
    public string Name => Param.Name;
    public bool Required => Param.Required;
    public IEnumerable<string> Aliases => Param.Aliases;
    public IEnumerable<string> DescriptionStrings => Param.DescriptionStrings;
    public IEnumerable<Type> ValuesTypes => Param.ParamsTypes;

    internal UserParamInfo(ParamInfo param)
    {
        Param = param;
    }

    public override string ToString() => Param.ToString();

    internal static UserParamInfo Create(ParamInfo param)
        => new UserParamInfo(param);
}