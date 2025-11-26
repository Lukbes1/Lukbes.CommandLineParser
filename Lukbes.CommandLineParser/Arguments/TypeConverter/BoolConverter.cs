namespace Lukbes.CommandLineParser.Arguments.TypeConverter;

public class BoolConverter : IConverter<bool>
{
    public string? TryConvert(string? value, out bool result)
    {
        bool hasError = bool.TryParse(value, out result);
        if (!hasError)
        {
            return $"{value} could not be parsed as a bool";
        }

        return null;
    }
}