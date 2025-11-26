namespace Lukbes.CommandLineParser.Extracting;

/// <summary>
/// Extracts the raw string of the identifier and the raw string of the value
/// </summary>
public interface IValuesExtractor
{
    /// <summary>
    /// Extracts the raw string of the identifier and the raw string of the value
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    (Dictionary<string, string?> identifierAndValues, List<string> errors) Extract(string[] args);
}