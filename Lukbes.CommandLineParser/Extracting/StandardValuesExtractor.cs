using System.Text.RegularExpressions;
using Lukbes.CommandLineParser.Arguments;

namespace Lukbes.CommandLineParser.Extracting;

/// <summary>
/// Classic Argument format extractor
/// Extracts the following: -r, --Argumentblah, -r="djdj", -r='something', -something=valuexyz, etc. 
/// </summary>
public sealed partial class StandardValuesExtractor : IValuesExtractor
{
    [GeneratedRegex(   @"^\s*(?<dashType>--?)(?<key>[A-Za-z][A-Za-z0-9_-]*)(?:=(?<value>""[^""]*""|'[^']*'|[^ \t""']+))?\s*$",
        RegexOptions.CultureInvariant | RegexOptions.NonBacktracking)]
    private static partial Regex REGEX();
    public (Dictionary<ArgumentIdentifier, string?> identifierAndValues, List<string> errors) Extract(string[] args)
    {
        Dictionary<ArgumentIdentifier, string?> identifierAndValues = new();
        List<string> errors = new();
        foreach (var arg in args)
        {
            var match = REGEX().Match(arg);
            if (!match.Success)
            {
                if (CommandLineParser.WithExceptions)
                {
                    throw new CommandLineArgumentExtractionException(arg);
                }
                errors.Add(CommandLineArgumentExtractionException.CreateMessage(arg));
            }
            var dashType = match.Groups["dashType"].Value;
            var key = match.Groups["key"].Value;
            var value = match.Groups["value"].Value.Trim('\'', '"');
            if (string.IsNullOrEmpty(value))
            {
                value = !string.IsNullOrEmpty(key) ? "true" : null;
            }

            ArgumentIdentifier identifier = new(dashType == "-" ? key : null, dashType == "--" ? key : null);
            identifierAndValues.Add(identifier, value);
        }
        return (identifierAndValues, errors);
    }
}