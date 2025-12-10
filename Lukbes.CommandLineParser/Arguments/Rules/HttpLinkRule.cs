using System.Text.RegularExpressions;

namespace Lukbes.CommandLineParser.Arguments.Rules;

/// <summary>
/// Checks if the <see cref="Argument{T}"/> of type string is a valid Httplink File via <see cref="File.Exists"/>
/// </summary>
public sealed partial class HttpLinkRule : IRule<string>
{
    public string? Validate(Argument<string> argument)
    {
        return HttpLinkRegex().IsMatch(argument.Value!) ? null : $"The value '{argument.Value}' of '{argument.Identifier}' was not a http link";
    }

    [GeneratedRegex(@"^https?://[^\s/$.?#].[^\s]*$", RegexOptions.IgnoreCase)]
    private static partial Regex HttpLinkRegex();
}