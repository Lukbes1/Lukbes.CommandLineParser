namespace Lukbes.CommandLineParser.Arguments.TypeConverter;

/// <summary>
/// This interface is used to define how an argument should be converted. <br/>
/// T is the returníng type of the conversion
/// </summary>
/// <typeparam name="T">The type of the return value</typeparam>
public interface IConverter<T>
{
    /// <summary>
    /// Converts the value into the given type
    /// </summary>
    /// <param name="value"></param>
    /// <param name="result"></param>
    /// <returns>null if successfully, error otherwise</returns>
    string? TryConvert(string? value, out T? result);
    
}