using Lukbes.CommandLineParser.Arguments.Dependencies;

namespace Lukbes.CommandLineParser.Arguments;

/// <summary>
/// Interface for the defining parts of an Argument. Only used to capture many Arguments no matter the type T in a list inside the commandlineparser
/// </summary>
public interface IArgument
{
    /// <summary>
    /// True if this argument has a value, false otherwise
    /// </summary>
    bool HasValue { get; }
    
    /// <summary>
    /// The value of the argument
    /// </summary>
    object? Value { get; }
    
    /// <summary>
    /// The underlying type of the Value
    /// </summary>
    Type ValueType { get; }
    
    /// <summary>
    /// Unique identifier with short and long name
    /// </summary>
    ArgumentIdentifier Identifier { get; }
    
    /// <summary>
    /// Applies the converter and the ruleset onto the value.
    /// Sets the internal <see cref="Value"/> property if successfully
    /// </summary>
    /// <param name="value">The raw string value</param>
    /// <returns>empty if successfully, list of errors otherwise</returns>
    List<string> Apply(string? value);
    
    /// <summary>
    /// Validates the argument by checking its dependencies. This process happens after <see cref="Apply"/>
    /// </summary>
    /// <param name="allOtherArgs">All other args except this</param>
    /// <returns>empty if successfully, list of errors otherwise</returns>
    List<string> ValidateDependencies(HashSet<IArgument> allOtherArgs);
}