namespace Lukbes.CommandLineParser.Arguments.TypeConverter;

public class CharConverter : IConverter<char>
{
    public string? TryConvert(string? value, out char result)
    {
        bool hasError =  char.TryParse(value, out result);
        if (!hasError)
        {
            return $"{value} could not be parsed as a char";
        }

        return null;
    }
}