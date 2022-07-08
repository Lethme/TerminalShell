# TerminalShell
Simple in use cmd-like console app interface library

How to use:

After instaliing

```C#
using var shell = Terminal.Shell.Create("Shelly");
await shell.RunAsync(); // Or you can use Shell.Run() method to run shell synchronously
```

Shell.Create() method will take all commands contexts from current assembly, so these contexts must be public.
If you need to initialize shell with the exact contexts you should use Shell.UseBuilder() method instead.
Then you can add any amount of contexts by using ShellBuilder.AddCommandsContext<>() method.
And finally you have to initialize shell with ShellBuilder.BuildShell() method.

```C#
using var shell = Terminal.Shell.UseBuilder()
    .AddCommandsContext<Commands>()
    .AddCommandsContext<AnotherCommands>()
    .BuildShell("Shelly");
await shell.RunAsync();
```

**Remember that this does the same thing as Terminal.Shell.Create() method!**

```C#
using var shell = Terminal.Shell.UseBuilder()
    .BuildShell("Shelly");
await shell.RunAsync();
```

Commands context is just a public class which must be inherited from CommandsContext class.
Then you can just create any commands inside of the context like this:

For example, this command requires a set of double values and calculates the average value of specified set:

```C#
using Terminal.Attributes;
using Terminal.Commands;
using Terminal.Containers;

namespace ShellExample;

public class Commands : CommandsContext
{
    [Command("Average")]
    [Aliases("Avg")]
    [Description("Calculates the average of any amount of values")]
    [Param("Values", true, typeof(double[]))]
    [ParamAliases("Values", "Vals", "V")]
    [ParamDescription("Values", "Any amount of values")]
    public double Average(ParamsCollection paramsCollection)
    {
        var values = paramsCollection.GetArray<double>("V"); // Arrays are only available in the end of any parameter types and only once!
        return values.Aggregate((acc, next) => acc + next) / values.Length;
        
        // If you want to get any other parameter value use ParamsCollection.Get<>() method.
    }
    
    // If command method returns any value it will be displayed in your console.
    // As it uses Object.ToString() method you can return any custom object from command method overriding ToString() method
}
```

After running shell you can invoke your command like this:

```
Shelly> average --values 1 2 3
Or
Shelly> average 1 2 3

Here you can use 'avg' as command name instead of 'average' and 'vals' or 'v' as parameter's name instead of 'values'
```

**Remember that shell is not case sensitive!**

If your command have only one required parameter or only one parameter at all, it's not necessary to specify param name.

Command attribute just tells shell what name current command will have.
With Aliases attribute you can set a short versions of any command.
Param attributes are used to tell shell what parameters current command can use.
ParamAliases attributes are just the same as Aliases attribute so you can set a short versions of parameters names.
Description attributes are describing your command.

Every command which has at least one parameter must take a ParamsCollection object.
In the command methods you can get any parameter by using ParamsCollection.Get<>() method but if you have an array in the param types sequence you have to use ParamsCollection.GetArray<>() method instead.

Also you can observe current command's data as name, aliases and etc.
You can do this in command methods by using CommandsContext.Command property.

If you need any custom output you can set void as command method's return type and then use CommandsContext.Log() or CommandsContext.LogLine() methods.
