using System.Security.AccessControl;

namespace Lukbes.CommandLineParser.Arguments;

public sealed class CommandLineArgumentRequiredException<T>(Argument<T> argument) : CommandLineArgumentException(CreateMessage(argument))
{
    public static string CreateMessage(Argument<T> argument)
    {
        return $"Error: \"{argument.Identifier}\" is required but was not provided";
    }
}