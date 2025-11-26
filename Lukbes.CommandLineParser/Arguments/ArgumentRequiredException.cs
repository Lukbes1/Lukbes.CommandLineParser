using System.Security.AccessControl;

namespace Lukbes.CommandLineParser.Arguments;

public class ArgumentRequiredException<T>(Argument<T> argument) : Exception(CreateMessage(argument))
{
    public static string CreateMessage(Argument<T> argument)
    {
        return $"Error: \"{argument.Identifier}\" is required but was not provided";
    }
}