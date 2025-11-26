namespace Lukbes.CommandLineParser.Arguments.TypeConverter;

public class StringConverter : IConverter<string>
{
    public string? TryConvert(string? value, out string? result)
    {
        result = value;
        return null;
    }
}