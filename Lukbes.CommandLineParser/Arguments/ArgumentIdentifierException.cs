namespace Lukbes.CommandLineParser.Arguments;

public class ArgumentIdentifierException(ArgumentIdentifier identifier) : Exception(CreateMessage(identifier))
{
    public static string CreateMessage(ArgumentIdentifier identifier)
    {
        return $"Error: The identifier must have at least the short or long version defined. actually: short was \"{(identifier.ShortIdentifier is null ? "null" : "not null")}\" and long was \"{(identifier.LongIdentifier is null ? "null" : "not null")}\"";
    }
}