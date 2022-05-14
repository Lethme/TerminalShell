# TerminalShell
Simple in use cmd-like console app interface library

How to use:

After downloading

```C#
using Terminal;

using var shell = Shell.Create("Shelly");
await shell.RunAsync(); // Or you can use Shell.Run() method to run shell synchronously
```

Shell.Create() method will take all commands contexts from current assembly, so these contexts must be public.
If you need to initialize shell with the exact contexts you should use Shell.UseBuilder() method instead.
Then you can add any amount of contexts by using ShellBuilder.AddCommandsContext<>() method.
And finally you have to initialize shell with ShellBuilder.BuildShell() method.

```C#
using var shell = Shell.UseBuilder()
    .AddCommandsContext<Commands>()
    .AddCommandsContext<AnotherCommands>()
    .BuildShell("Shelly");
await shell.RunAsync();
```

Commands context is just a public class which must be inherited from CommandsContext class.
Then you can just create any commands inside of the context like this:

```C#
using Terminal.Attributes;
using Terminal.Commands;
using Terminal.Containers;
using Terminal.Extensions;

namespace ShellTest;

public class Commands : CommandsContext
{
    [Command("Average")]
    [Aliases("Avg")]
    [Description("Calc an average of any amount of integer numbers")]
    [Param("Numbers", true, typeof(int[]))]
    [ParamAliases("Numbers", "Nums", "N")]
    [ParamDescription("Numbers", "Array of integer numbers")]
    public double Average(ParamsCollection paramsCollection)
    {
        var numbers = paramsCollection.GetArray<int>("n"); // Arrays are only available in the end of any parameter
        return Math.Round(numbers!.Aggregate((acc, cur) => acc + cur).Cast<double>() / numbers!.Length, 3);
    }
    
    // If command method returns any value it will be displayed in your console.
    // As it uses Object.ToString() method you can return any custom object from command method overriding ToString() method
}
```

Command attribute just tells shell what name current command will have.
With Aliases attribute you can set a short versions of any command.
Param attributes are used to tell shell what parameters current command can use.
ParamAliases attributes are just the same as Aliases attribute so you can set a short versions of parameters names.

Every command which has at least one parameter must take a ParamsCollection object.
In the command methods you can get any parameter by using ParamsCollection.Get<>() method but if you have an array in the param types sequence you have to use ParamsCollection.GetArray<>() method instead.

Also you can observe current command's data as name, aliases and etc.
You can do this in command methods by using CommandsContext.Command property.

If you need any custom output you can set void as command method's return type and then use CommandsContext.Log() or CommandsContext.LogLine() methods.
