namespace Lukbes.CommandLineParser.Arguments;

public sealed class CommandLineArgumentDoesNotExistException(ArgumentIdentifier identifier) : CommandLineArgumentException(CreateMessage(identifier))
{
    
    public static string CreateMessage(ArgumentIdentifier identifier)
    {
        return $"Argument '{identifier}' is not a valid argument";
    }
}