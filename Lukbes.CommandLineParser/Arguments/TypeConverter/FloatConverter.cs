namespace Lukbes.CommandLineParser.Arguments.TypeConverter;

public class FloatConverter : IConverter<float>
{
    public string? TryConvert(string? value, out float result)
    {
        bool hasError = float.TryParse(value, out result);
        if (!hasError)
        {
            return $"\"{value}\" could not be parsed as a float";
        }

        return null;
    }
}