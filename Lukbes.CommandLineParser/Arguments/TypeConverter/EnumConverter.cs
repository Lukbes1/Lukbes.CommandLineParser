using System.Text;

namespace Lukbes.CommandLineParser.Arguments.TypeConverter;

public class EnumConverter<T> : IConverter<T> where T : Enum
{
    public string? TryConvert(string? value, out T? result)
    {
        result = default;
        if (string.IsNullOrEmpty(value))
        {
            return $"Expected '{EnumNames<T>()}', but found '{value}'";
        }

        if (Enum.TryParse(typeof(T), value, true, out var val))
        {
            result = (T)val;
            return null;
        }

        return $"value '{value}' could not be converted to type '{typeof(T).GetFriendlyTypeName()}', options are {EnumNames<T>()} ";
    }

    public static string EnumNames<TEnum>() where TEnum : Enum
    {
        return string.Join(",", Enum.GetNames(typeof(T)));
    }
}