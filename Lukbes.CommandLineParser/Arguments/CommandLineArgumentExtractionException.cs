namespace Lukbes.CommandLineParser.Arguments;

public sealed class CommandLineArgumentExtractionException(ArgumentIdentifier identifier, object? expected, object? actual) : CommandLineArgumentException(CreateMessage(identifier, expected, actual))
{


    public static string CreateMessage(ArgumentIdentifier identifier, object? expected, object? actual)
    {
        return $"'{identifier}' did not satisfy expected {expected}. Actual is {actual}";
    }
}