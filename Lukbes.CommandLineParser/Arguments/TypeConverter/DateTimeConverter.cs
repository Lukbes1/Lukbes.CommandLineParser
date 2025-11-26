namespace Lukbes.CommandLineParser.Arguments.TypeConverter;

public class DateTimeConverter : IConverter<DateTime>
{
    public string? TryConvert(string? value, out DateTime result)
    {
        bool hasError =  DateTime.TryParse(value, out result);
        if (!hasError)
        {
            return $"{value} could not be parsed as a DateTime";
        }

        return null;
    }
}