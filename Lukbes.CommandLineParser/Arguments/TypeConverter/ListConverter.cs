namespace Lukbes.CommandLineParser.Arguments.TypeConverter;

public class ListConverter<T>(IConverter<T> itemConverter) : IConverter<List<T>>
{
    public string? TryConvert(string? value, out List<T>? result)
    {
        if (value is null)
        {
            result = [];
            return null;
        }
        List<T> items = new List<T>();
        result = items;
        foreach (var item in value.Split(","))
        {
            var convertError = itemConverter.TryConvert(item, out T? itemResult);
            if (convertError is null)
            {
                items.Add(itemResult!);
            }
            else
            {
                return convertError;
            }
        }

        return null;
    }
}