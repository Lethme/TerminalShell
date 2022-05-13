using Terminal.Commands;
using Terminal.Core;

namespace Terminal.Extensions;

internal static class StringExtensions
{
    internal static IEnumerable<string> ParseCommandLine(this string commandLine)
    {
        if (commandLine == String.Empty)
            return Array.Empty<string>();
        
        var matches = ShellBase.ParsingRegex.Matches(commandLine);

        return matches
            .Select(match => match
                .Groups
                .Values
                .Last(g => g.Success)
                .Value
            );
    }
}