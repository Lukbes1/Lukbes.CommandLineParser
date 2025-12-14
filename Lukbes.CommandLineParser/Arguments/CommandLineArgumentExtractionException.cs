namespace Lukbes.CommandLineParser.Arguments;

public sealed class CommandLineArgumentExtractionException(string identifier) : CommandLineArgumentException(CreateMessage(identifier))
{
    public static string CreateMessage(string identifier)
    {
        return $"'{identifier}' did not satisfy the right format.";
    }
}