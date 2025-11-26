namespace Lukbes.CommandLineParser.Arguments.Rules;

/// <summary>
/// A Rule is applied on arguments to check whether a certain condition is true or not.
/// </summary>
/// <typeparam name="T">The type of the Argument on which it can be applied</typeparam>
public interface IRule<T>
{
    /// <summary>
    /// Validates the <see cref="Argument{T}"/> 
    /// </summary>
    /// <param name="argument">The argument to be validated</param>
    /// <returns>Null if successfully, errormessage otherwise</returns>
    string? Validate(Argument<T> argument);
}