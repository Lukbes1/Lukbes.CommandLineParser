using Lukbes.CommandLineParser.Arguments.TypeConverter;

namespace Lukbes.CommandLineParser.Arguments;

public sealed class CommandLineArgumentConvertException<T>(ArgumentIdentifier identifier, string triedValue, string convertError) : CommandLineArgumentException(CreateMessage(identifier, triedValue, convertError))
{

    public static string CreateMessage(ArgumentIdentifier identifier, string triedValue, string convertError)
    {
        return $"Error: Argument \"{identifier}\" could not convert value \"{triedValue}\" to type \"{typeof(T).Name}\". Actual: {convertError}";
    }
}