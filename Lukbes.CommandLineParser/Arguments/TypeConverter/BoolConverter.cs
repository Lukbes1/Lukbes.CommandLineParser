namespace Lukbes.CommandLineParser.Arguments.TypeConverter;

public sealed class BoolConverter : IConverter<bool>
{
    public string? TryConvert(string? value, out bool result)
    {
        bool success = bool.TryParse(value, out result);
        if (!success)
        {
            return $"\"{value}\" could not be parsed as a bool";
        }

        return null;
    }
}