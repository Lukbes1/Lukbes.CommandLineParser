namespace Lukbes.CommandLineParser.Arguments;

public sealed class CommandLineArgumentDependencyException(string error) : CommandLineArgumentException(CreateMessage(error))
{
    public static string CreateMessage(string error)
    {
        return error;
    }
}