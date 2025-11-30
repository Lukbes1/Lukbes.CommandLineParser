namespace Lukbes.CommandLineParser.Arguments.TypeConverter;

public static class DefaultConverterFactory
{
    private static readonly Dictionary<Type, object> _converters = new()
    {
        [typeof(int)] = new IntConverter(),
        [typeof(double)] = new DoubleConverter(),
        [typeof(bool)] = new BoolConverter(),
        [typeof(string)] = new StringConverter(),
        [typeof(char)] = new CharConverter(),
        [typeof(float)] = new FloatConverter(),
        [typeof(DateTime)] = new DateTimeConverter()
    };
    
    /// <summary>
    /// Returns all already defined types that have a default Converter
    /// </summary>
    public static List<Type> GetTypes => _converters.Keys.ToList(); 

    /// <summary>
    /// Try creating an <see cref="IConverter{T}"/> from type <typeparamref name="T"/> 
    /// </summary>
    /// <param name="converter"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>false if type did not exist, true otherwise</returns>
    public static bool TryCreate<T>(out IConverter<T>? converter)
    {
        var success =_converters.TryGetValue(typeof(T), out var converterObject);
        converter = converterObject as IConverter<T>;
        return success;
    }

    /// <summary>
    /// Add a new Default converter. The <paramref name="converter"/> will be chosen, if the type of the <see cref="Argument{T}"/> matches <typeparamref name="T"/> and no other Converter is found <br/>
    /// </summary>
    /// <param name="converter"></param>
    /// <typeparam name="T"></typeparam>
    public static bool TryAdd<T>(IConverter<T> converter)
    {
       return _converters.TryAdd(typeof(T), converter);   
    }

    /// <summary>
    /// Removes the predefined converter of type <paramref name="converterType"/> if it exists.
    /// </summary>
    /// <param name="converterType"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>True if existing converter was deleted, false otherwise</returns>
    public static bool TryRemove<T>(Type converterType)
    {
        return _converters.Remove(converterType);
    }
}