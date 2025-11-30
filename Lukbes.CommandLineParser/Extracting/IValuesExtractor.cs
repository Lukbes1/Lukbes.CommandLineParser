using Lukbes.CommandLineParser.Arguments;

namespace Lukbes.CommandLineParser.Extracting;

/// <summary>
/// Extracts the raw string of the identifier and the raw string of the value
/// </summary>
public interface IValuesExtractor
{
    /// <summary>
    /// Extracts the raw string (wrapped in ArgumentIdentifier) of the identifier and the raw string of the value
    /// </summary>
    /// <param name="args"></param>
    /// <returns>A tuple of a dictionary with identifiers and their values, aswell as any errors that occurred</returns>
    (Dictionary<ArgumentIdentifier, string?> identifierAndValues, List<string> errors) Extract(string[] args);
}