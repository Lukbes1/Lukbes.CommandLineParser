namespace Lukbes.CommandLineParser.Arguments;

public class ArgumentExtractionException(ArgumentIdentifier identifier, object? expected, object? actual) : Exception(CreateMessage(identifier, expected, actual))
{


    public static string CreateMessage(ArgumentIdentifier identifier, object? expected, object? actual)
    {
        return $"Error: \"{identifier}\" did not satisfy expected {expected}. Actual is {actual}";
    }
}