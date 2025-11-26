using System.Text.RegularExpressions;

namespace Lukbes.CommandLineParser.Extracting;

/// <summary>
/// Classic Argument format extractor
/// Extracts the following: -r, --Argumentblah, -r="djdj", -r='something', -something=valuexyz, etc. 
/// </summary>
public class StandardValuesExtractor : IValuesExtractor
{
    private static readonly Regex REGEX = new(
        @"^\s*--?(?<key>[A-Za-z][A-Za-z0-9_-]*)(?:=(?<value>['""]?[^'""\s]+['""]?))?\s*$",
        RegexOptions.Compiled);
    public (Dictionary<string, string?> identifierAndValues, List<string> errors) Extract(string[] args)
    {
        Dictionary<string, string?> identifierAndValues = new();
        List<string> errors = new();
        foreach (var arg in args)
        {
            var match = REGEX.Match(arg);
            if (!match.Success) continue;
            var key = match.Groups["key"].Value;
            var value = match.Groups["value"].Value.Trim('\'', '"');
            if (string.IsNullOrEmpty(value))
            {
                if (!string.IsNullOrEmpty(key))
                {
                    value = "true";
                }
                else
                {
                    value = null;
                }
            }
            identifierAndValues.Add(key, value);
        }
        return (identifierAndValues, errors);
    }
}