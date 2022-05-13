using System.Collections;
using System.Linq;
using Microsoft.VisualBasic.CompilerServices;
using Terminal.Exceptions;
using Terminal.Extensions;

namespace Terminal.Containers;

public class ParamsCollection : IEnumerable<ParamContainer>, IDisposable
{
    private IEnumerable<ParamContainer> Params { get; }
    public bool Empty => Params.Any();

    public object[] this[string name] => GetParam(name);
    public object? this[string name, int index] => Get(name, index);

    public ParamsCollection(params ParamContainer[] commandParams)
    {
        this.Params = commandParams;
        CheckParams();
    }
    
    public ParamsCollection(IEnumerable<ParamContainer> commandParams)
    {
        this.Params = commandParams;
        CheckParams();
    }

    public ParamsCollection(Dictionary<string, (IEnumerable<string> Aliases, LinkedList<object> Values)> paramsDictionary)
    {
        this.Params = paramsDictionary
            .Select(param =>
                ParamContainer.Create(
                    param.Key,
                    param.Value.Aliases,
                    param.Value.Values
                )
            );

        CheckParams();
    }

    private void CheckParams()
    {
        if (HasDuplicated())
        {
            throw new ParamsDuplicationException(
                $"These parameters were duplicated:\n" +
                $"{String.Join("\n", GetDuplicated())}"
            );
        }
    }

    private bool HasDuplicated()
    {
        return Params
            .GroupBy(p => p.Name)
            .Any(g => g.Count() > 1);
    }
    private IEnumerable<string> GetDuplicated()
    {
        var duplicated = Params
            .GroupBy(p => p.Name)
            .Where(g => g.Count() > 1);

        if (duplicated.Any())
            return duplicated.Select(g => g.Key);

        return Array.Empty<string>();
    }

    public T? Get<T>(string paramName, int paramIndex)
    {
        if (typeof(T).IsArray)
            throw new UnsupportedParamCollectionTypeException(
                $"You can't get array params with 'Get' method. Use 'GetArray' instead."
            );
        
        var param = Get(paramName, paramIndex);

        if (param == default)
            return default;

        var castResult = param.TryCast<T>();

        if (!castResult.Success)
            return default;

        return castResult.Obj;
    }

    private object? Get(string paramName, int paramIndex)
    {
        var paramsArray = GetParam(paramName);

        if (
            paramsArray.Length == 0 ||
            paramIndex < 0 ||
            paramIndex > paramsArray.Length - 1
        )
        {
            return default;
        }

        var obj = paramsArray[paramIndex];
        
        if (obj.GetType().IsArray)
            throw new UnsupportedParamCollectionTypeException(
                $"You can't get array params with 'Get' method. Use 'GetArray' instead."
            );
        
        return obj;
    }

    private object[]? Get(string paramName)
    {
        var paramsArray = GetParam(paramName);

        if (paramsArray.Length == 0)
        {
            return default;
        }

        return paramsArray;
    }

    public T[]? GetArray<T>(string paramName)
    {
        var param = GetArray(paramName);

        if (param == default)
            return default;

        var castResult = param.TryCastArray<T>();

        if (!castResult.Success)
            return default;

        return castResult.Obj;
    }

    private object[]? GetArray(string paramName)
    {
        var paramsArray = GetParam(paramName);
        return paramsArray.FirstOrDefault(param => param.GetType().IsArray) as object[];
    }

    public bool Has(string paramName)
    {
        return Params
            .FirstOrDefault(p =>
                p.Name == paramName ||
                p.Aliases.Any(alias => alias == paramName)
            ) != default;
    }

    private object[] GetParam(string paramName)
    {
        var param = Params
            .FirstOrDefault(p => 
                p.Name.ToLower() == paramName.ToLower() ||
                p.Aliases.Any(alias => alias.ToLower() == paramName.ToLower())
            );

        if (param == default)
            return Array.Empty<object>();

        return param.Values;
    }

    public static ParamsCollection Create(params ParamContainer[] commandParams)
        => new ParamsCollection(commandParams);
    public static ParamsCollection Create(IEnumerable<ParamContainer> commandParams)
        => new ParamsCollection(commandParams);
    public static ParamsCollection Create(Dictionary<string, (IEnumerable<string> Aliases, LinkedList<object> Values)> paramsDictionary)
        => new ParamsCollection(paramsDictionary);

    public IEnumerator<ParamContainer> GetEnumerator()
    {
        return Params.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Dispose()
    {
    }
}