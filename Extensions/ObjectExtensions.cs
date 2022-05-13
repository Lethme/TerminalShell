using System.Reflection;

namespace Terminal.Extensions;

public static class ObjectExtensions
{
    public static void Log(this object obj)
    {
        if (obj.ToString() != String.Empty)
            Console.WriteLine(obj);
    }
    public static void LogLine(this object obj)
    {
        if (obj.ToString() != String.Empty)
            Console.WriteLine($"{obj}\n");
    }
    public static T Cast<T>(this object obj)
    {
        return (T)Convert.ChangeType(obj, typeof(T));
    }

    public static object Cast(this object obj, Type type)
    {
        return Convert.ChangeType(obj, type);
    }

    internal static bool HasArray(this object[] objects)
    {
        return objects.Any(obj => obj.GetType().IsArray);
    }

    internal static (bool Success, T[]? Obj) TryCastArray<T>(this object[] objects)
    {
        try
        {
            var castedArray = Array.ConvertAll<object, T>(objects, obj => obj.TryCast<T>().Obj!);
            return (true, castedArray);
        }
        catch (Exception ex)
        {
            return (false, default);
        }
    }
    
    internal static (bool Success, object[]? Obj) TryCastArray(this object[] objects, Type type)
    {
        try
        {
            var castedArray = Array.ConvertAll(objects, obj => obj.TryCast(type).Obj!);
            return (true, castedArray);
        }
        catch (Exception ex)
        {
            return (false, default);
        }
    }
    
    internal static (bool Success, T? Obj) TryCast<T>(this object obj)
    {
        try
        {
            var castedObj = (T)Convert.ChangeType(obj, typeof(T));
            return (true, castedObj);
        }
        catch
        {
            return (false, default);
        }
    }
    
    internal static (bool Success, object? Obj) TryCast(this object obj, Type type)
    {
        try
        {
            var castedObj = Convert.ChangeType(obj, type);
            return (true, castedObj);
        }
        catch
        {
            return (false, default);
        }
    }
    
    internal static IEnumerable<MethodInfo> GetMethods<TAttribute>(this object obj)
        where  TAttribute : Attribute
    {
        return obj
            .GetType()
            .GetMethods()
            .Where(method => method.GetCustomAttribute(typeof(TAttribute)) != default);
    }

    internal static IEnumerable<TAttribute> GetAttributes<TAttribute>(this object obj)
        where TAttribute : Attribute
    {
        return obj
            .GetType()
            .GetCustomAttributes(typeof(TAttribute))
            .Select(attr => (TAttribute)Convert.ChangeType(attr, typeof(TAttribute)));
    }

    internal static bool HasAttribute<TAttribute>(this object obj)
        where TAttribute : Attribute
    {
        return obj
            .GetType()
            .GetCustomAttributes(typeof(TAttribute))
            .Any();
    }
}