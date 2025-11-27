namespace Lukbes.CommandLineParser.Arguments.TypeConverter;

public static class DefaultConverterFactory
{
    private static readonly Dictionary<Type, object> _converters = new()
    {
        [typeof(int)] = new IntConverter(),
        [typeof(double)] = new DoubleConverter(),
        [typeof(bool)] = new BoolConverter(),
        [typeof(string)] = new StringConverter(),
        [typeof(DateTime)] = new DateTimeConverter(),
    };

    public static bool TryCreate<T>(out IConverter<T>? converter)
    {
        var success =_converters.TryGetValue(typeof(T), out var converterObject);
        converter = converterObject as IConverter<T>;
        return success;
    }

    public static void AddConverter<T>(IConverter<T> converter)
    {
        
    }
    
    /*
    public static IConverter<T>? Create<T>()
    {
        return typeof(T) switch
        {
            var t when t == typeof(int) => new IntConverter() as IConverter<T>,
            var t when t == typeof(string) => new StringConverter() as IConverter<T>,
            var t when t == typeof(Boolean) => new BoolConverter() as IConverter<T>,
            _ => null
        };
    }*/
}