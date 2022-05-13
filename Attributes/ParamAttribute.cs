using System.Linq;
using Terminal.Core;
using Terminal.Exceptions;
using Terminal.Extensions;

namespace Terminal.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ParamAttribute : Attribute
{
    public string Name { get; }
    public bool Required { get; }
    public IEnumerable<Type> ParamsTypes { get; }

    public ParamAttribute(string name, params Type[] paramsTypes)
    {
        if (paramsTypes.Any(paramType => !ShellBase.AllowedTypes.Contains(paramType)))
        {
            var unsupportedTypes = paramsTypes
                .Where(paramType => !ShellBase.AllowedTypes.Contains(paramType) &&
                                    !paramType.IsShellArray());

            if (unsupportedTypes != null && unsupportedTypes.Any())
            {
                throw new UnsupportedParamTypeException(
                    $"These param types are not supported:\n" +
                    $"{String.Join('\n', unsupportedTypes)}"
                );
            }
        }

        if (paramsTypes.Any(paramType => paramType.IsShellArray()))
        {
            if (paramsTypes
                .Select((paramType, index) => new {Type = paramType, Index = index})
                .Where(paramType => paramType.Type.IsShellArray())
                .Any(paramType => paramType.Index != paramsTypes.Length - 1)
            )
            {
                throw new ParamTypeException(
                    $"Arrays can only appear in the end of param types"
                );
            }
        }
        
        this.Name = name;
        this.Required = false;
        this.ParamsTypes = paramsTypes;
    }
    public ParamAttribute(string name, bool required, params Type[] paramsTypes)
    {
        if (paramsTypes.Any(paramType => !ShellBase.AllowedTypes.Contains(paramType)))
        {
            var unsupportedTypes = paramsTypes
                .Where(paramType => !ShellBase.AllowedTypes.Contains(paramType) &&
                                    !paramType.IsShellArray());

            if (unsupportedTypes != null && unsupportedTypes.Any())
            {
                throw new UnsupportedParamTypeException(
                    $"These param types are not supported:\n" +
                    $"{String.Join('\n', unsupportedTypes)}"
                );
            }
        }
        
        if (paramsTypes.Any(paramType => paramType.IsShellArray()))
        {
            if (paramsTypes
                .Select((paramType, index) => new {Type = paramType, Index = index})
                .Where(paramType => paramType.Type.IsShellArray())
                .Any(paramType => paramType.Index != paramsTypes.Length - 1)
               )
            {
                throw new ParamTypeException(
                    $"Arrays can only appear in the end of param types"
                );
            }
        }

        this.Name = name;
        this.Required = required;
        this.ParamsTypes = paramsTypes;
    }

    public override string ToString()
    {
        return $"Param name: {Name}\nRequired: {Required}\nParams: {String.Join(" ", ParamsTypes.Select(t => t.Name))}";
    }
}