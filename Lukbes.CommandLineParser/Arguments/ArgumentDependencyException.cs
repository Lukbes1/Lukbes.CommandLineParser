namespace Lukbes.CommandLineParser.Arguments;

public class ArgumentDependencyException(string error) : Exception(CreateMessage(error))
{
    public static string CreateMessage(string error)
    {
        return error;
    }
}