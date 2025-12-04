using System.ComponentModel;
using System.Formats.Tar;
using System.Net.Security;
using System.Text;
using Lukbes.CommandLineParser.Arguments.Dependencies;
using Lukbes.CommandLineParser.Arguments.Rules;
using Lukbes.CommandLineParser.Arguments.TypeConverter;

namespace Lukbes.CommandLineParser.Arguments;

/// <summary>
/// This class defines your commandline argument, its supposed type and a set of rules and dependencies which it enforces
/// </summary>
/// <typeparam name="T">The type of the Arguments supposed value</typeparam>
public class Argument<T> : IArgument
{
    /// <summary>
    /// The Value of the argument
    /// </summary>
    public T? Value { get; private set; } = default;

    object? IArgument.Value => Value;
    
    public Type ValueType => typeof(T);
    
    public bool HasValue { get; private set; }
    
    public T? DefaultValue { get; private set; }
    
    public bool HasDefaultValue { get; private set; }
    
    public ArgumentIdentifier Identifier { get; private set; } = new();
    
    public string? Description { get; private set; }
    
    public bool IsRequired { get; private set; }
    
    public IConverter<T>? Converter { get; private set; }
    
    private readonly List<IDependency> _dependencies = new();

    private readonly HashSet<IRule<T>> _rules = new();
    
    /// <summary>
    /// Gets a new instance of an <see cref="ArgumentBuilder{T}"/> 
    /// </summary>
    /// <returns></returns>
    public static ArgumentBuilder<T> Builder() => new();
    
    private Argument()
    {
    }

    /// <summary>
    /// Use this constrcutor to build an argument with a builder
    /// </summary>
    /// <param name="builder"></param>
    public Argument(Func<ArgumentBuilder<T>, Argument<T>> builder) : this(builder.Invoke(Builder()))
    { 
    }
    
    /// <summary>
    /// Shallow copy constructor. This has same references as other 
    /// </summary>
    /// <param name="other"></param>
    private Argument(Argument<T> other)
    {
        Value = other.Value;
        HasDefaultValue = other.HasDefaultValue;
        HasValue = other.HasDefaultValue;
        Identifier = other.Identifier;
        IsRequired = other.IsRequired;
        Converter = other.Converter;
        Description = other.Description;
        _rules = other._rules;
        _dependencies = other._dependencies;
        DefaultValue = other.DefaultValue;
    }
    
    /// <summary>
    /// <inheritdoc cref="IArgument.Apply"/>
    /// </summary>
    /// <param name="value"><inheritdoc cref="IArgument.Apply"/></param>
    /// <returns> <inheritdoc cref="IArgument.Apply"/></returns>
    /// <exception cref="CommandLineArgumentRequiredException{T}">If string is null or empty and this IsRequired</exception>
    /// <exception cref="CommandLineArgumentConvertException{T}">If <paramref name="value"/> could not be converted to <see cref="T"/></exception>
    /// <exception cref="CommandLineArgumentRuleException">If a Rule failed</exception>
    public List<string> Apply(string? value)
    {
        List<string> errors = new();
        
        if (string.IsNullOrEmpty(value))
        {
            if (IsRequired)
            {
                if (CommandLineParser.WithExceptions)
                {
                    throw new CommandLineArgumentRequiredException<T>(this);
                } 
                errors.Add(CommandLineArgumentRequiredException<T>.CreateMessage(this));
                return errors;
            }

            if (!HasDefaultValue)
            {
                HasValue = false;
            }
            return []; 
        }
        
        string? convertError = Converter!.TryConvert(value, out var result);
        if (!string.IsNullOrEmpty(convertError))
        {
            errors.Add(CommandLineArgumentConvertException<T>.CreateMessage(Identifier, value!, convertError));
        }
        if (convertError is not null && DefaultValue is null)
        {
            HasValue = false;
            if (CommandLineParser.WithExceptions)
            {
                throw new CommandLineArgumentConvertException<T>(Identifier, value!, convertError);
            }
        }
        else
        {
            Value = result ?? DefaultValue;
            HasValue = true;
            foreach (var rule in _rules)
            {
                var error = rule.Validate(this);
                if (error is not null)
                {
                    if (CommandLineParser.WithExceptions)
                    {
                        throw new CommandLineArgumentRuleException(Identifier, value!, error);
                    }
                    errors.Add(CommandLineArgumentRuleException.CreateMessage(Identifier, value!, error));
                }
            }
        }
        return errors;
    }
    
    /// <summary>
    /// <inheritdoc cref="IArgument.ValidateDependencies"/>
    /// </summary>
    /// <param name="allOtherArgs"><inheritdoc cref="IArgument.ValidateDependencies"/></param>
    /// <returns><inheritdoc cref="IArgument.ValidateDependencies"/></returns>
    /// <exception cref="CommandLineArgumentDependencyException">If a dependency fails</exception>
    public List<string> ValidateDependencies(HashSet<IArgument> allOtherArgs)
    {
        if (_dependencies.Count == 0)
        {
            return [];
        }
        List<string> result = new();

        foreach (var dependency in _dependencies)
        {
            List<string> dependencyErrors = dependency.Check(this, allOtherArgs);
            if (dependencyErrors.Count > 0)
            {
                if (CommandLineParser.WithExceptions)
                {
                    throw new CommandLineArgumentDependencyException(dependencyErrors[0]); 
                }

                foreach (var error in dependencyErrors)
                {
                    result.Add(error);
                }
            }
        }
        return result;
    }
    
    protected bool Equals(Argument<T> other)
    {
        return Identifier.Equals(other.Identifier);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Argument<T>)obj);
    }

    public override int GetHashCode()
    {
        return Identifier.GetHashCode();
    }

    /// <summary>
    /// Gives back a string that is a standard way of writing arguments:<br/>
    /// optional -> [-a, --arg]: int | My Description 
    /// required -> -a, --arg: string | My other Description
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        StringBuilder result = new();
        if (!IsRequired)
        {
            result.Append($"[{Identifier}]").Append(' ');
        }
        else
        {
            result.Append(Identifier).Append(' ');
        }

        result.Append($": {GetFriendlyTypeName(typeof(T))} | {Description} ");

        if (HasDefaultValue)
        {
            result.Append($"(Default: {DefaultValue})");
        }
        
        return result.ToString();
    }
    
    public static string GetFriendlyTypeName(Type type)
    {
        if (type.IsGenericType)
        {
            string typeName = type.Name;
            int backtickIndex = typeName.IndexOf('`');
            if (backtickIndex > 0)
            {
                typeName = typeName.Substring(0, backtickIndex);
            }
              

            string genericArgs = string.Join(", ", type.GetGenericArguments()
                .Select(GetFriendlyTypeName));

            return $"{typeName}<{genericArgs}>";
        }

        return type.Name;
    }


    /// <summary>
    /// Used to build an <see cref="Argument{T}"/>
    /// </summary>
    /// <typeparam name="TArg"></typeparam>
    public class ArgumentBuilder<TArg>
    {
        private readonly Argument<TArg> _argument = new();

        /// <summary>
        /// Sets the <see cref="ArgumentIdentifier"/> of the <see cref="Argument{T}"/>
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public ArgumentBuilder<TArg> Identifier(ArgumentIdentifier identifier)
        {
            _argument.Identifier = identifier;
            return this;
        }

        /// <summary>
        /// Sets the short identifier of the <see cref="Argument{T}"/>
        /// </summary>
        /// <param name="shortIdentifier"></param>
        /// <returns></returns>
        public ArgumentBuilder<TArg> ShortIdentifier(string shortIdentifier)
        {
            _argument.Identifier.ShortIdentifier = shortIdentifier;
            return this;
        }
        
        /// <summary>
        /// Sets the long identifier of the <see cref="Argument{T}"/>
        /// </summary>
        /// <param name="longIdentifier"></param>
        /// <returns></returns>
        public ArgumentBuilder<TArg> LongIdentifier(string longIdentifier)
        {
            _argument.Identifier.LongIdentifier = longIdentifier;
            return this;
        }

        /// <summary>
        /// Sets the description of the <see cref="Argument{T}"/>
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public ArgumentBuilder<TArg> Description(string? description)
        {
            _argument.Description = description;
            return this;
        }
        
        /// <summary>
        /// Sets the <see cref="Argument{T}"/> to required
        /// </summary>
        /// <returns></returns>
        public ArgumentBuilder<TArg> IsRequired()
        {
            _argument.IsRequired = true;
            return this;
        }

        /// <summary>
        /// Sets a default value 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ArgumentBuilder<TArg> DefaultValue(TArg value)
        {
            _argument.DefaultValue = value;
            _argument.Value = value;
            _argument.HasDefaultValue = true;
            _argument.HasValue = true;
            return this;
        }

        /// <summary>
        /// Sets the converter of the  <see cref="Argument{T}"/>
        /// </summary>
        /// <param name="converter"></param>
        /// <returns></returns>
        public ArgumentBuilder<TArg> Converter(IConverter<TArg> converter)
        {
            _argument.Converter = converter;
            return this;
        }

        /// <summary>
        /// Set the type of the List. <br/>
        /// Use this method if you want to use default converters of type <typeparamref name="TListItemType"/> <br/>
        /// Necessary because the Converter can not be automatically infered through the argument Type. <br/>
        /// </summary>
        /// <typeparam name="TListItemType"></typeparam>
        /// <returns></returns>
        /// <exception cref="CommandLineArgumentConverterException{TListItemType}">If a default converter for <typeparamref name="TListItemType"/> does not exist</exception>
        public ArgumentBuilder<TArg> Converter<TListItemType>()
        {
            var converter = DefaultConverterFactory.CreateListConverter<TListItemType>();
            if (converter is null)
            {
                throw new CommandLineArgumentConverterException<TListItemType>();
            }

            _argument.Converter = converter as IConverter<TArg>;
            return this;
        }
        
        /// <summary>
        /// Set the type of the List. <br/>
        /// Use this method if you want a custom converter for the <typeparamref name="TListItemType"/> <br/>
        /// Necessary because the Converter can not be automatically infered through the argument Type.
        /// </summary>
        /// <typeparam name="TListItemType"></typeparam>
        /// <returns></returns>
        public ArgumentBuilder<TArg> Converter<TListItemType>(IConverter<TListItemType> converter)
        {
            if (converter is null)
            {
                throw new BuilderPropertyNullOrEmptyException<TArg>(nameof(converter));
            }
            _argument.Converter = new ListConverter<TListItemType>(converter) as IConverter<TArg>;
            return this;
        }
        
        /// <summary>
        /// Adds a rule to the <see cref="Argument{T}"/>. Rules are applied after retrieving the value
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        public ArgumentBuilder<TArg> Rule(IRule<TArg> rule)
        {
            _argument._rules.Add(rule);
            return this;
        }
        
        #region Dependencies

        /// <summary>
        /// Adds a dependency to the <see cref="Argument{T}"/>
        /// </summary>
        /// <param name="dependency"></param>
        /// <returns></returns>
        public ArgumentBuilder<TArg> Dependency(IDependency dependency)
        {
            _argument._dependencies.Add(dependency);
            return this;
        }
        
        /// <summary>
        /// If this <see cref="Argument{T}"/> is present and has a value, also require all <paramref name="arguments"/>
        /// </summary>
        /// <param name="first"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public ArgumentBuilder<TArg> RequiresAll(IArgument first, params IArgument[] arguments)
        {
            _argument._dependencies.Add(new RequiresAll(first, arguments));
            return this;
        }
        
        /// <summary>
        /// If this <see cref="Argument{T}"/> is present and has a value, also require at least one of <paramref name="arguments"/>
        /// </summary>
        /// <param name="first"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public ArgumentBuilder<TArg> RequiresOneOf(IArgument first, params IArgument[] arguments)
        {
            _argument._dependencies.Add(new RequiresOneOf(first, arguments));
            return this;
        }
        #endregion
        
       /// <summary>
       /// Builds the <see cref="Argument{T}"/> and gives it back.
       /// </summary>
       /// <returns><see cref="Argument{TArg}"/></returns>
       /// <exception cref="CommandLineArgumentIdentifierException">If no Identifier was set</exception>
       /// <exception cref="BuilderPropertyNullOrEmptyException{T}">If no default converter was found and no explicit converter was set</exception>
        public Argument<TArg> Build() 
        {
            if (!_argument.Identifier.Validate())
            {
                throw new CommandLineArgumentIdentifierException(_argument.Identifier);
            }

            if (_argument.Converter is null)
            {
                if (DefaultConverterFactory.TryCreate(out IConverter<TArg>? converter)) 
                {
                    _argument.Converter = converter;
                }
            }
            BuilderPropertyNullOrEmptyException<IConverter<TArg>>.ThrowIfNullOrEmpty(nameof(_argument.Converter), _argument.Converter);
            return _argument;
        }
        
    }
}