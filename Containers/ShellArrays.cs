using Terminal.Exceptions;
using Terminal.Extensions;
using Terminal.Services;

namespace Terminal.Containers;

internal static class ShellArrays
{
    private static Dictionary<string, List<object>> Arrays { get; } = new();

    internal static string Create(string shellName)
    {
        var arrayName = HashService.GetHashSha256(shellName, DateTime.Now.ToFileTime().ToString());
        
        if (Arrays.ContainsKey(arrayName))
        {
            throw new ShellArrayDuplicationException(
                $"Tried to create existing array: {arrayName}"
            );
        }
        
        Arrays.Add(arrayName, new());
        
        return arrayName;
    }

    internal static void Append(string arrayName, params object[] objects)
    {
        if (Arrays.ContainsKey(arrayName))
        {
            foreach (var obj in objects)
            {
                Arrays[arrayName].Add(obj);
            }
        }
    }

    internal static long Length(string arrayName)
    {
        if (Arrays.ContainsKey(arrayName))
            return Arrays[arrayName].Count;
        
        return default;
    }

    internal static T[]? Get<T>(string arrayName)
    {
        if (Arrays.ContainsKey(arrayName))
        {
            var castResult = Arrays[arrayName].ToArray().TryCastArray<T>();

            if (castResult.Success)
                return castResult.Obj!;
        }

        return default;
    }

    internal static object[]? Get(string arrayName)
    {
        if (Arrays.ContainsKey(arrayName))
            return Arrays[arrayName].ToArray();
        
        return default;
    }

    internal static Type? GetType(string arrayName)
    {
        return Arrays.ContainsKey(arrayName) 
            ? Arrays[arrayName].ToArray().GetType()
            : default;
    }

    internal static Type? GetElementType(string arrayName)
    {
        return Arrays.ContainsKey(arrayName) 
            ? Arrays[arrayName].ToArray().GetType().GetElementType() 
            : default;
    }

    internal static bool Remove(string arrayName)
    {
        return Arrays.Remove(arrayName);
    }

    internal static bool Exists(string arrayName)
    {
        return Arrays.ContainsKey(arrayName);
    }
}