namespace Lukbes.CommandLineParser.Arguments.TypeConverter;

public class IntConverter : IConverter<int>
{
    public string? TryConvert(string? value, out int result)
    {
        bool hasError = int.TryParse(value, out result);
        if (!hasError)
        {
            return $"\"{value}\" could not be parsed as an integer";
        }

        return null;
    }
    
}