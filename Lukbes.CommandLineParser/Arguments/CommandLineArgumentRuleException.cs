namespace Lukbes.CommandLineParser.Arguments;

public sealed class CommandLineArgumentRuleException(ArgumentIdentifier identifier, string triedValue, string ruleError) : CommandLineArgumentException(CreateMessage(identifier, triedValue, ruleError))
{
    
    public static string CreateMessage(ArgumentIdentifier identifier, string triedValue, string ruleError)
    {
        return $"Error: rule failed for \"{identifier}\". Tried value: \"{triedValue}\". Rule error: {ruleError}";
    }
}