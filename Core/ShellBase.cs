using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Terminal.Commands;
using Terminal.Containers;
using Terminal.Enums;
using Terminal.Exceptions;
using Terminal.Extensions;
using Terminal.Services;

namespace Terminal.Core;

public abstract class ShellBase : IDisposable
{
    internal const string DefaultParamsMarker = "--";
    internal const string DefaultCommandMarker = ">";
    internal const string ParsingRegularExpression = "[^\\s\"\']+|\"([^\"]*)\"|\'([^\']*)\'";
    internal const string DefaultParamName = "";
    internal static Type[] AllowedTypes { get; } = new[]
    {
        typeof(bool),
        typeof(byte),
        typeof(sbyte),
        typeof(char),
        typeof(decimal),
        typeof(double),
        typeof(float),
        typeof(int),
        typeof(uint),
        typeof(nint),
        typeof(nuint),
        typeof(long),
        typeof(ulong),
        typeof(short),
        typeof(ushort),
        typeof(string)
    };
    internal Version Version => typeof(ShellBase).Assembly.GetName().Version;
    internal static Regex ParsingRegex { get; } = new Regex(ParsingRegularExpression);
    protected private List<CommandsContext> Contexts { get; private set; }
    protected private IEnumerable<CommandInfo> Commands => Contexts.GetCommands();
    internal string CommandMarker { get; }
    internal string ParamsMarker { get; }
    internal string Name { get; set; }
    private Task RunTask { get; }
    private CancellationTokenSource RunTaskTokenSource { get; }

    private protected ShellBase(string name, string commandMarker = DefaultCommandMarker, string paramsMarker = DefaultParamsMarker)
    {
        var contexts = CommandsContext
            .ContextsTypes
            .Prepend(typeof(DefaultCommandsContext));
        
        CheckCommandsContextsTypes(contexts);

        Contexts = contexts.CreateContexts().ToList();
        Name = name;
        CommandMarker = commandMarker;
        ParamsMarker = paramsMarker;
        RunTaskTokenSource = new CancellationTokenSource();
        RunTask = new Task(RunAction, RunTaskTokenSource.Token);

        CheckCommandsContexts(Contexts);
    }
    private protected ShellBase(IEnumerable<Type> contexts, string name, string commandMarker = DefaultCommandMarker, string paramsMarker = DefaultParamsMarker)
    {
        contexts = contexts.Prepend(typeof(DefaultCommandsContext));
        
        CheckCommandsContextsTypes(contexts);

        Contexts = contexts.CreateContexts().ToList();
        Name = name;
        CommandMarker = commandMarker;
        ParamsMarker = paramsMarker;
        RunTaskTokenSource = new CancellationTokenSource();
        RunTask = new Task(RunAction, RunTaskTokenSource.Token);

        CheckCommandsContexts(Contexts);
    }

    private void CheckCommandsContextsTypes(IEnumerable<Type> contexts)
    {
        if (!contexts.HasDefaultConstructors())
        {
            throw new CommandsContextException(
                $"Commands contexts must contain default constructor:\n" +
                $"{String.Join("\n", contexts.GetNoDefaultConstructorsContexts())}"
            );
        }

        if (!contexts.IsDefaultConstructorsPublic())
        {
            throw new CommandsContextException(
                $"Commands contexts default constructor must be public:\n" +
                $"{String.Join("\n", contexts.GetNonPublicDefaultConstructorsContexts())}"
            );
        }

        if (contexts.HasNonDefaultConstructors())
        {
            throw new CommandsContextException(
                $"Commands contexts can only contain default constructors:\n" +
                $"{String.Join("\n", contexts.GetNonDefaultConstructorsContexts())}"
            );
        }
    }
    
    private void CheckCommandsContexts(IEnumerable<CommandsContext> contexts)
    {
        if (!contexts.Any())
        {
            throw new CommandsContextException(
                $"Shell '{Name}' had no commands contexts"
            );
        }

        if (contexts.HasEmptyContext())
        {
            throw new CommandsContextException(
                $"These contexts had no commands:\n" +
                $"{String.Join("\n", contexts.GetEmptyContexts().Select(ctx => ctx.GetType().FullName))}"
            );   
        }

        if (contexts.HasProperties())
        {
            var properties = contexts.GetProperties();
            throw new CommandsContextException(
                $"Properties are not allowed in commands contexts:\n" +
                $"{String.Join("\n", properties.Select(p => $"{p.ContextName}: {String.Join(" ", p.Properties)}"))}"
            );
        }
        
        if (contexts.HasFields())
        {
            var fields = contexts.GetFields();
            throw new CommandsContextException(
                $"Fields are not allowed in commands contexts:\n" +
                $"{String.Join("\n", fields.Select(f => $"{f.ContextName}: {String.Join(" ", f.Fields)}"))}"
            );
        }

        if (contexts.HasPrivateCommandMethods())
        {
            var privateMethods = contexts.GetPrivateCommandMethods();
            throw new CommandsContextException(
                $"Non-public methods are not allowed in commands contexts:\n" +
                $"{String.Join("\n", privateMethods.Select(m => $"{m.ContextName}: {String.Join(" ", m.Methods)}"))}"
            );
        }

        if (contexts.HasDuplicatedMethods())
        {
            var duplicatedMethods = contexts.GetDuplicatedMethods();
            throw new CommandsContextException(
                $"Command methods duplication is not allowed\n" +
                $"{String.Join("\n", duplicatedMethods.Select(method => $"{method.MethodName}: {String.Join(" ", method.Contexts)}"))}"
            );
        }

        if (contexts.HasNonCommandMethods())
        {
            var nonCommandMethods = contexts.GetNonCommandMethods();
            throw new CommandsContextException(
                $"Non-command methods are not allowed in commands contexts:\n" +
                $"{String.Join("\n", nonCommandMethods.Select(m => $"{m.ContextName}: {String.Join(" ", m.Methods)}"))}"
            );
        }

        if (!contexts.IsCommandMethodsValid())
        {
            var invalidCommandsInfos = contexts.GetInvalidCommands().Select(cmd =>
            {
                return $"Context: {cmd.ContextName}\n" +
                       $"Command: {cmd.Name}\n" +
                       $"Method: {cmd.Method.Name}\n";
            });
            throw new InvalidCommandDeclarationException(
                $"These commands were invalid:\n" +
                $"{String.Join("\n", invalidCommandsInfos)}"
            );
        }

        if (contexts.HasDuplicatedCommands())
        {
            var duplicatedCommands = contexts.GetDuplicatedCommands();
            throw new CommandsDuplicationException($"\nMultiple commands declaration:\n{String.Join("\n", duplicatedCommands)}");
        }

        if (contexts.HasDuplicatedAliases())
        {
            var duplicatedAliases = contexts.GetDuplicatedAliases();
            var duplicatedCommands = duplicatedAliases
                .Select(alias => new { Alias = alias, DuplicatedCommands = contexts.GetCommands(alias) });
            
            throw new AliasesDuplicationException(
                $"Multiple command aliases declaration\n" +
                $"{String.Join("\n", duplicatedCommands.Select(cmd => $"{cmd.Alias}: {String.Join(" ", cmd.DuplicatedCommands.Select(c => c.Name))}"))}"
            );
        }

        if (contexts.HasParamsConflicts())
        {
            var commandsWithConflictingParams = contexts.GetCommandsWithConflictingParams();
            throw new ParamsDuplicationException(
                $"Some parameters had multiple definition in commands:\n" +
                $"{String.Join("\n", commandsWithConflictingParams)}"
            );
        }

        if (contexts.HasDuplicatedParams())
        {
            var duplicatedParams = contexts.GetDuplicatedParams();
            throw new ParamsDuplicationException(
                $"Multiple params declaration\n" +
                $"{String.Join("\n", duplicatedParams.Select(param => $"{param.CommandName}: {String.Join(" ", param.DuplicatedParams)}"))}"
            );
        }
        
        if (contexts.HasParamsWithDuplicatedAliases())
        {
            var duplicatedParams = contexts.GetParamsWithDuplicatedAliases();
            throw new ParamsDuplicationException(
                $"Multiple params aliases declaration\n" +
                $"{String.Join("\n", duplicatedParams.Select(param => $"{param.CommandName}: {String.Join(" ", param.DuplicatedParams)}"))}"
            );
        }
    }
    
    private Command? ParseCommand(string commandLine)
    {
        var commandParams = commandLine.ParseCommandLine();

        if (!commandParams.Any())
            return default;

        var commandName = commandParams.First();
        var command = Contexts.GetCommand(commandName);
        
        if (command == default)
            throw new CommandNotFoundException(
                $"Unknown command: {commandName}"
           );

        commandParams = commandParams.Skip(1);

        var paramsDictionary = GetParamsDictionary(commandParams);
        
        if (
            paramsDictionary.ContainsKey(DefaultParamName) &&
            paramsDictionary.Count() > 1
        )
            throw new CommandParsingException(
                $"Default parameters can only be used without any other parameters"
            );

        var castedParamsDictionary = CheckParamsDictionary(command, paramsDictionary);

        return Command.Create(command.Name, ParamsCollection.Create(castedParamsDictionary));
    }

    private Dictionary<string, LinkedList<string>> GetParamsDictionary(IEnumerable<string> commandParams)
    {
        var paramsDictionary = new Dictionary<string, LinkedList<string>>();
        var paramFound = false;
        var currentParam = DefaultParamName;
        
        commandParams.ForEach((p, i) =>
        {
            if (p.StartsWith(ParamsMarker) && p.Length > ParamsMarker.Length)
            {
                paramFound = true;
                currentParam = p.Replace(ParamsMarker, "");

                if (paramsDictionary.ContainsKey(currentParam))
                    throw new CommandParsingException(
                        $"Duplicated parameter declaration: {currentParam}"
                    );

                paramsDictionary.Add(currentParam, new LinkedList<string>());
            }
            else
            {
                if (!paramFound)
                {
                    paramsDictionary.Add(currentParam, new LinkedList<string>());
                    paramFound = true;
                }
                
                paramsDictionary[currentParam].AddLast(p);
            }
        });

        return paramsDictionary;
    }

    private Dictionary<string, (IEnumerable<string> Aliases, LinkedList<object> Values)> CheckParamsDictionary(CommandInfo cmd, Dictionary<string, LinkedList<string>> paramsDictionary)
    {
        if (paramsDictionary.Count > cmd.Params.Count())
        {
            throw new CommandValidatingException(
                $"Parameters count mismatch"
            );
        }
        else
        {
            var requiredParams = cmd.GetRequiredParams();

            var actualRequiredParams = paramsDictionary
                .Keys
                .Where(key =>
                    requiredParams.Any(requiredKey => requiredKey.Is(key))
                );

            var requiredParamsCount = requiredParams.Count();
            var actualRequiredParamsCount = actualRequiredParams.Count();

            if (
                requiredParamsCount != actualRequiredParamsCount &&
                !paramsDictionary.ContainsKey(DefaultParamName)
            )
            {
                throw new CommandValidatingException(
                    $"Required parameters count mismatch"
                );   
            }

            if (
                paramsDictionary.ContainsKey(DefaultParamName)
            )
            {
                if (
                    (requiredParamsCount == 0 && cmd.Params.Count() > 1) ||
                    requiredParamsCount > 1
                )
                    throw new CommandValidatingException(
                        $"You can only use default parameter if you have only one required parameter or only one parameter at all."
                    );
                
                if (cmd.Params.Count() == 1)
                {
                    var tempParams = paramsDictionary[DefaultParamName];
                    paramsDictionary.Remove(DefaultParamName);
                    paramsDictionary.Add(cmd.Params.First().Name, tempParams);
                }
                else if (requiredParamsCount == 1)
                {
                    var tempParams = paramsDictionary[DefaultParamName];
                    paramsDictionary.Remove(DefaultParamName);
                    paramsDictionary.Add(requiredParams.First().Name, tempParams);
                }
            }
        }

        var castedParamsDictionary = new Dictionary<string, (IEnumerable<string> Aliases, LinkedList<object> Values)>();

        paramsDictionary.ForEach(param =>
        {
            if (!cmd.HasParam(param.Key))
                throw new CommandValidatingException(
                    $"Unknown parameter for command \"{cmd.Name}\": {param.Key}"
                );

            var currentParam = cmd.GetParam(param.Key);
            castedParamsDictionary.Add(currentParam!.Name, (currentParam.Aliases, new LinkedList<object>()));

            if (param.Value.Count < currentParam.ParamsTypes.Count())
            {
                throw new CommandValidatingException(
                    $"Values count mismatch: {param.Key}"
                );   
            }

            var paramValuesTypes = currentParam.ParamsTypes.ToArray();
            var paramValues = param.Value.ToArray();
            var paramsArrayHash = ShellArrays.Create(Name);
            for (int i = 0, len = paramValues.Length; i < len; i++)
            {
                if (i > paramValuesTypes.Length - 1 || (i < paramValuesTypes.Length && paramValuesTypes[i].IsShellArray()))
                {
                    if (!paramValues[i].TryCast(ShellArrays.GetElementType(paramsArrayHash)!).Success)
                        throw new CommandValidatingException(
                            $"Value type mismatch: \"{paramValues[i]}\" is not {ShellArrays.GetElementType(paramsArrayHash)}"
                        );

                    ShellArrays.Append(paramsArrayHash, paramValues[i].Cast(ShellArrays.GetElementType(paramsArrayHash)!));

                    if (i == len - 1)
                        castedParamsDictionary[currentParam.Name].Values.AddLast(ShellArrays.Get(paramsArrayHash)!);
                }
                else
                {
                    if (!paramValues[i].TryCast(paramValuesTypes[i]).Success)
                        throw new CommandValidatingException(
                            $"Value type mismatch: \"{paramValues[i]}\" is not {paramValuesTypes[i].Name}"
                        );
            
                    castedParamsDictionary[currentParam.Name].Values.AddLast(paramValues[i].Cast(paramValuesTypes[i]));   
                }
            }

            ShellArrays.Remove(paramsArrayHash);

            // param.Value.ForEach((value, index) =>
            // {
            //     if (!value.TryCast(paramValuesTypes[index]).Success)
            //         throw new CommandValidatingException(
            //             $"Value type mismatch: \"{value}\" is not {paramValuesTypes[index].Name}"
            //         );
            //
            //     castedParamsDictionary[currentParam.Name].Values.AddLast(value.TryCast(paramValuesTypes[index]).Obj!);
            // });
        });

        return castedParamsDictionary;
    }

    private object? Invoke(Command command)
    {
        var cmd = Contexts.GetCommand(command.Name)!;

        switch (cmd.Name)
        {
            case DefaultCommands.Help: return cmd.Invoke(Contexts, command.Params);
            case DefaultCommands.Commands: return cmd.Invoke(Contexts, command.Params);
            case DefaultCommands.Clear: return cmd.Invoke(this);
            case DefaultCommands.Exit: return cmd.Invoke(this);
            default: return cmd.Params.Any() ? cmd.Invoke(command.Params) : cmd.Invoke();
        }
    }

    private object? Invoke(string command)
    {
        var cmd = ParseCommand(command)!;
        return Invoke(cmd);
    }

    private string Enter()
    {
        var command = String.Empty;
        do
        {
            var shellMessage = Console.CursorLeft == 0
                ? $"{Name}{CommandMarker} "
                : $"\n{Name}{CommandMarker} ";
            
            Console.Write(shellMessage);
            command = Console.ReadLine();
        } while (command == String.Empty);

        return command!;
    }

    private void HandleException(Action action)
    {
        try
        {
            action.Invoke();
        }
        catch (Exception e)
        {
            var exception = e;
            
            //var exceptionTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == "Terminal.Exceptions").ToArray();
            
            while (exception.InnerException != null && exception.GetType().Namespace != "Terminal.Exceptions")
            {
                exception = exception.InnerException;
            }
            
            Console.WriteLine($"{exception.Message}\n");
        }
    }

    private void RunAction()
    {
        Clear();
        
        while (true)
        {
            HandleException(() =>
            {
                var command = Enter();
                var result = Invoke(command);
                
                 if (result != null) Console.WriteLine($"{result}\n");
            });

            GC.Collect();
            
            if (RunTaskTokenSource.IsCancellationRequested)
                return;
        }
    }

    public async Task RunAsync()
    {
        RunTask.Start();
        await RunTask.WaitAsync(CancellationToken.None);
    }
    
    public void Run()
    {
        RunTask.Start();
        RunTask.Wait();
    }

    public void Exit()
    {
        RunTaskTokenSource.Cancel();
    }

    public void Clear()
    {
        Console.Clear();
        Console.WriteLine($"Shell v{Version}");
        Console.WriteLine($"Use 'help <command name>' to see the available commands.\n");
    }

    private void SetTerminalName(string shellName)
    {
        if (RunTask.Status == TaskStatus.Running)
            Console.Title = shellName;
    }

    public void Dispose()
    {
        foreach (var ctx in Contexts)
        {
            ctx.Dispose();
        }

        Contexts.Clear();
        Contexts = null;
        
        RunTask.Dispose();
        RunTaskTokenSource.Dispose();
        
        GC.Collect();
    }
}