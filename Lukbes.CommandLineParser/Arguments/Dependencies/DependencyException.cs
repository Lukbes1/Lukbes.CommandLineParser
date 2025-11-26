namespace Lukbes.CommandLineParser.Arguments.Dependencies;

public class DependencyException(string error) : Exception(CreateMessage(error))
{

    public static string CreateMessage(string error)
    {
        return error;
    }
}