namespace Lukbes.CommandLineParser.Arguments;

public class CommandLineArgumentUniqueException(IArgument newArg) : CommandLineArgumentException(CreateMessage(newArg))
{


    public static string CreateMessage(IArgument newArg)
    {
        return $"The argument {newArg.Identifier} is already specified";
    }
}