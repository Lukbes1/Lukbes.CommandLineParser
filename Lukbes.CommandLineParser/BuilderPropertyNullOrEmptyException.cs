using System.Collections;

namespace Lukbes.CommandLineParser;

/// <summary>
/// Exception that happens, if a builder did not have all required properties. Will still be thrown even if <see cref="CommandLineParser.WithExceptions"/> is set to false because the cmdlineparser cant run without it
/// </summary>
/// <typeparam name="T">The type of the object that was missing</typeparam>
public class BuilderPropertyNullOrEmptyException<T>(string propertyName) : Exception(CreateMessage(propertyName))
{
    public static void ThrowIfNullOrEmpty(string propertyName, T? val)
    {
        if (val is null || val is IList list && list.Count == 0)
        {
            throw new BuilderPropertyNullOrEmptyException<T>(propertyName);
        }
    }
    public static string CreateMessage(string propertyName)
    {
        return $"Property '{propertyName}' of type '{typeof(T).GetFriendlyTypeName()}' is required and must not be null or empty";
    }
    
}