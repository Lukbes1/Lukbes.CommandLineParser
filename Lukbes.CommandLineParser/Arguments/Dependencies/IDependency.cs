namespace Lukbes.CommandLineParser.Arguments.Dependencies;

/// <summary>
/// This <see cref="IDependency"/>Offers the ability to create a relationship between the argument it is used on and all other args <br/>
/// </summary>
public interface IDependency
{
    /// <summary>
    /// Check the dependency 
    /// </summary>
    /// <param name="argument"></param>
    /// <param name="otherArgs"></param>
    /// <returns>null if successfully, list of errors otherwise</returns>
    List<string> Check(IArgument argument, HashSet<IArgument> otherArgs);
}