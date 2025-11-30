namespace Lukbes.CommandLineParser.Arguments.TypeConverter;

public sealed class DoubleConverter : IConverter<double>
{
    public string? TryConvert(string? value, out double result)
    {
        
        bool hasError = double.TryParse(value, out result);
        if (!hasError)
        {
            return $"\"{value}\" could not be parsed as a double";
        }

        return null;
    }
}