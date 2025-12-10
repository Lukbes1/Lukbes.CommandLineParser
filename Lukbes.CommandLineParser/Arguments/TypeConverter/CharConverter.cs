namespace Lukbes.CommandLineParser.Arguments.TypeConverter;

public sealed class CharConverter : IConverter<char>
{
    public string? TryConvert(string? value, out char result)
    {
        bool success =  char.TryParse(value, out result);
        if (!success)
        {
            return $"'{value}' could not be parsed as a char";
        }

        return null;
    }
}