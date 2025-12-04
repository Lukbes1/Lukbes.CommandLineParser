namespace Lukbes.CommandLineParser.Arguments;

/// <summary>
/// Happens when the item converter of type <paramref name="T"/> can not be found
/// </summary>
/// <param name="message"></param>
/// <typeparam name="T"></typeparam>
public class CommandLineArgumentConverterException<T>() : CommandLineArgumentException(CreateMessage())
{


    public static string CreateMessage()
    {
        return
            $"A default converter of type {typeof(T).Name} does not exist. Register your own for this type or set the Converter with the other Converter method";
    }
}