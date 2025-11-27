using System.ComponentModel;
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
    public T? Value { get; private set; } = default;
    
    public bool HasValue { get; private set; }
    
    public ArgumentIdentifier Identifier { get; private set; } = new();
    
    public string? Description { get; private set; }
    
    public bool IsRequired { get; private set; }

    public List<IDependency> Dependencies { get; private set; } = new();
    public IConverter<T>? Converter { get; private set; }

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
        Identifier = other.Identifier;
        IsRequired = other.IsRequired;
        Converter = other.Converter;
        Description = other.Description;
        _rules = other._rules;
        Dependencies = other.Dependencies;
    }
    
    public List<string> Apply(string? value)
    {
        List<string> errors = new();
        
        if (IsRequired && string.IsNullOrEmpty(value))
        {
            if (CommandLineParser.WithExceptions)
            {
                throw new ArgumentRequiredException<T>(this);
            } 
            errors.Add(ArgumentRequiredException<T>.CreateMessage(this));
        }

        string? convertError = Converter!.TryConvert(value, out var result);
        if (convertError is not null)
        {
            if (CommandLineParser.WithExceptions)
            {
                throw new ArgumentConvertException<T>(Identifier, value!, convertError);
            }
            errors.Add(ArgumentConvertException<T>.CreateMessage(Identifier, value!, convertError));
        }
        else
        {
            Value = result!;
            HasValue = true;
            foreach (var rule in _rules)
            {
                var error = rule.Validate(this);
                if (error is not null)
                {
                    if (CommandLineParser.WithExceptions)
                    {
                        throw new ArgumentRuleException(Identifier, value!, error);
                    }
                    errors.Add(ArgumentRuleException.CreateMessage(Identifier, value!, error));
                }
            }
        }
        return errors;
    }
    

    public List<string> ValidateDependencies(HashSet<IArgument> allOtherArgs)
    {
        if (Dependencies.Count == 0)
        {
            return [];
        }
        List<string> result = new();

        foreach (var dependency in Dependencies)
        {
            List<string> dependencyErrors = dependency.Check(this, allOtherArgs);
            if (dependencyErrors.Count > 0)
            {
                if (CommandLineParser.WithExceptions)
                {
                    throw new ArgumentDependencyException(dependencyErrors[0]); 
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

    public override string ToString()
    {
    //TODO 
     return Identifier.ToString();
    }


    /// <summary>
    /// Used to build an <see cref="Argument{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ArgumentBuilder<T>
    {
        private readonly Argument<T> _argument = new();

        /// <summary>
        /// Sets the <see cref="ArgumentIdentifier"/> of the <see cref="Argument{T}"/>
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public ArgumentBuilder<T> Identifier(ArgumentIdentifier identifier)
        {
            _argument.Identifier = identifier;
            return this;
        }

        /// <summary>
        /// Sets the short identifier of the <see cref="Argument{T}"/>
        /// </summary>
        /// <param name="shortIdentifier"></param>
        /// <returns></returns>
        public ArgumentBuilder<T> ShortIdentifier(string shortIdentifier)
        {
            _argument.Identifier.ShortIdentifier = shortIdentifier;
            return this;
        }
        
        /// <summary>
        /// Sets the long identifier of the <see cref="Argument{T}"/>
        /// </summary>
        /// <param name="longIdentifier"></param>
        /// <returns></returns>
        public ArgumentBuilder<T> LongIdentifier(string longIdentifier)
        {
            _argument.Identifier.LongIdentifier = longIdentifier;
            return this;
        }

        /// <summary>
        /// Sets the description of the <see cref="Argument{T}"/>
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public ArgumentBuilder<T> Description(string? description)
        {
            _argument.Description = description;
            return this;
        }
        
        /// <summary>
        /// Sets the  <see cref="Argument{T}"/> to required
        /// </summary>
        /// <returns></returns>
        public ArgumentBuilder<T> IsRequired()
        {
            _argument.IsRequired = true;
            return this;
        }

        /// <summary>
        /// Sets the converter of the  <see cref="Argument{T}"/>
        /// </summary>
        /// <param name="converter"></param>
        /// <returns></returns>
        public ArgumentBuilder<T> Converter(IConverter<T> converter)
        {
            _argument.Converter = converter;
            return this;
        }
        
        /// <summary>
        /// Adds a rule to the <see cref="Argument{T}"/>. Rules are applied after retrieving the value
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        public ArgumentBuilder<T> Rule(IRule<T> rule)
        {
            _argument._rules.Add(rule);
            return this;
        }

        /// <summary>
        /// Adds a dependency to the <see cref="Argument{T}"/>
        /// </summary>
        /// <param name="dependency"></param>
        /// <returns></returns>
        public ArgumentBuilder<T> Dependency(IDependency dependency)
        {
            _argument.Dependencies.Add(dependency);
            return this;
        }
        
        /// <summary>
        /// If this <see cref="Argument{T}"/> is present and has a value, also require all <paramref name="arguments"/>
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public ArgumentBuilder<T> RequiresAll(params IArgument[] arguments)
        {
            _argument.Dependencies.Add(new RequiresAll(arguments));
            return this;
        }
        
        /// <summary>
        /// If this <see cref="Argument{T}"/> is present and has a value, also require at least one of <paramref name="arguments"/>
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public ArgumentBuilder<T> RequiresOneOf(params IArgument[] arguments)
        {
            _argument.Dependencies.Add(new RequiresOneOf(arguments));
            return this;
        }
        
        /// <summary>
        /// Builds the <see cref="Argument{T}"/> and gives it back.
        /// </summary>
        /// <returns></returns>
        public Argument<T> Build() 
        {
            BuilderPropertyNullOrEmptyException<ArgumentIdentifier>.ThrowIfNullOrEmpty(nameof(_argument.Identifier), _argument.Identifier);
            if (!_argument.Identifier.Validate())
            {
                throw new ArgumentIdentifierException(_argument.Identifier);
            }

            if (_argument.Converter is null)
            {
                if (DefaultConverterFactory.TryCreate(out IConverter<T>? converter)) 
                {
                    _argument.Converter = converter;
                }
                BuilderPropertyNullOrEmptyException<IConverter<T>>.ThrowIfNullOrEmpty(nameof(_argument.Converter), _argument.Converter);
            }
            BuilderPropertyNullOrEmptyException<IConverter<T>>.ThrowIfNullOrEmpty(nameof(_argument.Converter), _argument.Converter);
            return _argument;
        }
        
    }
}