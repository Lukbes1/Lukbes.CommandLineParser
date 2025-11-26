namespace Lukbes.CommandLineParser.Arguments;

public class ArgumentRuleException(ArgumentIdentifier identifier, string triedValue, string ruleError) : Exception(CreateMessage(identifier, triedValue, ruleError))
{
    
    public static string CreateMessage(ArgumentIdentifier identifier, string triedValue, string ruleError)
    {
        return $"Error: rule failed for \"{identifier}\". Tried value: {triedValue}. Rule error: {ruleError}";
    }
}