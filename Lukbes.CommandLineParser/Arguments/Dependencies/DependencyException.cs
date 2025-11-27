namespace Lukbes.CommandLineParser.Arguments.Dependencies;

/// <summary>
/// Exception that gets thrown if an <see cref="IDependency"/> fails
/// </summary>
/// <param name="error"></param>
public class DependencyException(string error) : Exception(CreateMessage(error))
{

    public static string CreateMessage(string error)
    {
        return error;
    }
}