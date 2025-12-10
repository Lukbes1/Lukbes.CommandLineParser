namespace Lukbes.CommandLineParser.Arguments.TypeConverter;

/// <summary>
/// The factory for creating IConverters so you don't need to specify them
/// </summary>
public static class DefaultConverterFactory
{
    private static readonly Dictionary<Type, Lazy<object>> _converters = new()
    {
        [typeof(int)] = new Lazy<object>(() => new IntConverter()),
        [typeof(List<int>)] = new Lazy<object>(() => CreateListConverter<int>()!),
        [typeof(double)] = new Lazy<object>(() => new DoubleConverter()),
        [typeof(List<double>)] = new Lazy<object>(() => CreateListConverter<double>()!),
        [typeof(bool)] = new Lazy<object>(() => new BoolConverter()),
        [typeof(List<bool>)] = new Lazy<object>(() => CreateListConverter<bool>()!),
        [typeof(string)] = new Lazy<object>(() => new StringConverter()),
        [typeof(List<string>)] = new Lazy<object>(() => CreateListConverter<string>()!),
        [typeof(char)] = new Lazy<object>(() => new CharConverter()),
        [typeof(List<char>)] = new Lazy<object>(() => CreateListConverter<char>()!),
        [typeof(float)] = new Lazy<object>(() => new FloatConverter()),
        [typeof(List<float>)] = new Lazy<object>(() => CreateListConverter<float>()!),
        [typeof(DateTime)] = new Lazy<object>(() => new DateTimeConverter()),
        [typeof(List<DateTime>)] = new Lazy<object>(() => CreateListConverter<DateTime>()!),
    };
    
    /// <summary>
    /// Returns all already defined types that have a default Converter
    /// </summary>
    public static List<Type> Types => _converters.Keys.ToList(); 

    /// <summary>
    /// Try creating an <see cref="IConverter{T}"/> from type <typeparamref name="T"/> 
    /// </summary>
    /// <param name="converter"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>false if type did not exist, true otherwise</returns>
    public static bool TryGet<T>(out IConverter<T>? converter)
    {
        var success =_converters.TryGetValue(typeof(T), out var converterObject);
        if (success)
        {
            converter = converterObject!.Value as IConverter<T>;
            return true;
        }
        converter = null;
        return success;
    }

    /// <summary>
    /// Add a new Default converter. The <paramref name="converter"/> will be chosen, if the type of the <see cref="Argument{T}"/> matches <typeparamref name="T"/> and no other Converter is found <br/>
    /// </summary>
    /// <param name="converter"></param>
    /// <typeparam name="T"></typeparam>
    public static bool TryAdd<T>(IConverter<T> converter)
    {
       return _converters.TryAdd(typeof(T), new Lazy<object>(() => converter));   
    }
    
    /// <summary>
    /// Add a new Default List converter. If a Converter of Type <paramref name="TListItemType"/> is found, automatically creates a new ListConverter
    /// </summary>
    /// <typeparam name="TListItemType"></typeparam>
    /// <returns>true if could be added, false otherwise</returns>
    public static bool TryAddList<TListItemType>()
    {
        if (!_converters.ContainsKey(typeof(TListItemType)))
        {
            return false;
        }
        return _converters.TryAdd(typeof(List<TListItemType>), new Lazy<object>(() => CreateListConverter<TListItemType>()!));   
    }
    
    /// <summary>
    /// Add a new Default List converter
    /// </summary>
    /// <param name="converter"></param>
    /// <typeparam name="TListItemType"></typeparam>
    /// <returns>true if could be added, false otherwise</returns>
    public static bool TryAddList<TListItemType>(IConverter<TListItemType> converter)
    {
        return _converters.TryAdd(typeof(List<TListItemType>), new Lazy<object>(() => new ListConverter<TListItemType>(converter)));   
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

    /// <summary>
    /// Clears all Defaults
    /// </summary>
    public static void Clear()
    {
        _converters.Clear();
    }
    
    /// <summary>
    /// Get a new List Converter of type <paramref name="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>The <see cref="ListConverter{T}"/> of type <paramref name="T"/></returns>
    public static ListConverter<T>? CreateListConverter<T>()
    {
        if (!_converters.TryGetValue(typeof(T), out var converter))
        {
            return null;
        }
        return new ListConverter<T>(converter!.Value as IConverter<T>);
    }
}