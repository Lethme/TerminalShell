using System.Reflection;
using System.Linq;

namespace Terminal.Services;

public static class AttributesService
{
    public static IEnumerable<MethodInfo> GetMethodsByAttribute<TClass, TAttribute>()
        where TClass : class
        where TAttribute : Attribute
    {
        return typeof(TClass)
            .GetMethods()
            .Where(method => method.GetCustomAttribute(typeof(TAttribute)) != default);
    }
    
    public static IEnumerable<MethodInfo> GetMethodsByAttribute<TAttribute>(object obj)
        where TAttribute : Attribute
    {
        return obj
            .GetType()
            .GetMethods()
            .Where(method => method.GetCustomAttribute(typeof(TAttribute)) != default);
    }

    public static IEnumerable<TAttribute> GetMethodAttributes<TAttribute>(MethodInfo method)
        where TAttribute : Attribute
    {
        return method
            .GetCustomAttributes(typeof(TAttribute))
            .Select(attr => (TAttribute)Convert.ChangeType(attr, typeof(TAttribute)));
    }
}