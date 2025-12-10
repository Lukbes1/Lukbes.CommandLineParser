namespace Lukbes.CommandLineParser.Arguments.Rules;

/// <summary>
/// Checks if the <see cref="Argument{T}"/> of type string is an existing File via <see cref="File.Exists"/>
/// </summary>
public sealed class ExistingFileRule : IRule<string>
{
    public string? Validate(Argument<string> argument)
    {
        return File.Exists(argument.Value) ? null : $"File '{argument.Value}' does not exist";
    }
}