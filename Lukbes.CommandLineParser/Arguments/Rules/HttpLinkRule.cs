using System.Text.RegularExpressions;

namespace Lukbes.CommandLineParser.Arguments.Rules;

public class HttpLinkRule : IRule<string>
{
    public string? Validate(Argument<string> argument)
    {
        return Regex.IsMatch(argument.Value!, @"^https?://[^\s/$.?#].[^\s]*$", RegexOptions.IgnoreCase) ? null : $"Error: The value {argument.Value} of {argument.Identifier} was not a http link";
    }
}