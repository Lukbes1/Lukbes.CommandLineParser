namespace Lukbes.CommandLineParser.Arguments;

public sealed class CommandLineArgumentRuleException(ArgumentIdentifier identifier, string triedValue, string ruleError) : CommandLineArgumentException(CreateMessage(identifier, triedValue, ruleError))
{
    
    public static string CreateMessage(ArgumentIdentifier identifier, string triedValue, string ruleError)
    {
        return $"Rule failed for '{identifier}'. Tried value: '{triedValue}'. Rule: {ruleError}";
    }
}