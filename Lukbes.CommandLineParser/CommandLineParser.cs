using System.Runtime.CompilerServices;
using Lukbes.CommandLineParser.Arguments;
using Lukbes.CommandLineParser.Extracting;

namespace Lukbes.CommandLineParser
{
    public class CommandLineParser
    {
        public static bool WithExceptions;

        private readonly HashSet<IArgument> _arguments = new();
        
        public IValuesExtractor Extractor { get; private set; }
        
        public IArgument HelpArg { get; private set; }
        
        /// <summary>
        /// Gets a new instance of an <see cref="CommandLineParserBuilder"/> 
        /// </summary>
        /// <returns></returns>
        public static CommandLineParserBuilder Builder() => new();
    
        private CommandLineParser()
        {
        }

        /// <summary>
        /// Use this constructor to build an argument with a builder
        /// </summary>
        /// <param name="builder"></param>
        public CommandLineParser(Func<CommandLineParserBuilder, CommandLineParser> builder) : this(builder.Invoke(Builder()))
        { 
        }
    
        /// <summary>
        /// Shallow copy constructor. This has same references as other 
        /// </summary>
        /// <param name="other"></param>
        private CommandLineParser(CommandLineParser other)
        {
            _arguments = other._arguments;
            HelpArg = other.HelpArg;
            Extractor = other.Extractor;
        }
        
        public List<string> Parse(string[] args)
        {
            List<string> errors = new();
            var extractedValues = Extractor.Extract(args);
            errors.AddRange(extractedValues.errors);

            foreach (var extractedEntry in extractedValues.identifierAndValues)
            {
                IArgument? foundArgument = _arguments.FirstOrDefault(x => x.Identifier.Equals(extractedEntry.Key)); //Only the short OR long version has to match
                if (foundArgument is null)
                {
                    continue;
                }
                var applyErrors = foundArgument.Apply(extractedEntry.Value);
                errors.AddRange(applyErrors);
            }
            
            foreach (var argument in _arguments)
            {
                var allOtherArgs = _arguments.Where(a => !ReferenceEquals(a, argument)).ToHashSet();
                var dependencyErrors = argument.ValidateDependencies(allOtherArgs);
                errors.AddRange(dependencyErrors);
            }
            
           return errors;
        }

        /// <summary>
        /// Gets <see cref="Argument{T}"/> of type <typeparamref name="T"/>. 
        /// </summary>
        /// <param name="identifier">The identifier of the Argument</param>
        /// <param name="argument"><see cref="Argument{T}"/> or null if it could not be found, or Type is wrong.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>null if successfully, error otherwise</returns>
        public string? TryGetArgument<T>(ArgumentIdentifier identifier, out Argument<T>? argument)
        {
            argument = null;
            IArgument? arg = _arguments.FirstOrDefault(x => x.Identifier.Equals(identifier));
            if (arg is null)
            {
                return $"Error: Argument \"{identifier}\" could not be found.";
            }
            try
            {
                Type[] genericArgs = arg.GetType().GenericTypeArguments;
            
                if (genericArgs.Length is 0 or > 1)
                {
                    return $"Error: Argument \"{identifier}\" must have only one generic argument.";
                }

                if (genericArgs[0] != typeof(T))
                {
                    return
                        $"Error: Argument \"{identifier}\" must have the type  \"{typeof(T)}\". Actual: \"{genericArgs[0]}\"";
                }
                
                Argument<T> castedArg = (Argument<T>)arg;
                argument = castedArg;
                return null;  
            }
            catch (Exception e)
            {
                return $"Error: Argument \"{identifier}\" could not be casted. Actual: {e.Message}";
            }
        }

        /// <summary>
        /// Gets <see cref="Argument{T}"/> of type <typeparamref name="T"/>. 
        /// </summary>
        /// <param name="identifier">The identifier of the Argument</param>
        /// <param name="argument"><see cref="Argument{T}"/> or null if it could not be found, or Type is wrong.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>null if successfully, error otherwise</returns>
        public string? TryGetValue<T>(ArgumentIdentifier identifier,  out Argument<T>? argument)
        {
            var error = TryGetArgument<T>(identifier, out var arg);
            argument = arg;
            return error;
        }

        public class CommandLineParserBuilder
        {
            private readonly CommandLineParser _parser = new();

            /// <summary>
            /// Adds a new <see cref="Argument"/>
            /// </summary>
            /// <param name="argument"></param>
            /// <exception cref="BuilderPropertyNullOrEmptyException{T}"></exception>
            public CommandLineParserBuilder Argument(IArgument argument)
            {
                BuilderPropertyNullOrEmptyException<IArgument>.ThrowIfNullOrEmpty(nameof(argument), argument);
                _parser._arguments.Add(argument);
                return this;
            }
            
            /// <summary>
            /// Adds new Arguments <see cref="arguments"/>
            /// </summary>
            /// <param name="arguments"></param>
            /// <exception cref="BuilderPropertyNullOrEmptyException{T}"></exception>
            public CommandLineParserBuilder Arguments(params IArgument[] arguments)
            {
                BuilderPropertyNullOrEmptyException<IArgument[]>.ThrowIfNullOrEmpty(nameof(arguments), arguments);
                foreach (var argument in arguments)
                {
                    _parser._arguments.Add(argument);
                }
                return this;
            }

            
            /// <summary>
            /// Sets the Help <see cref="Argument"/>
            /// </summary>
            /// <returns></returns>
            /// <exception cref="BuilderPropertyNullOrEmptyException{T}"></exception>
            public CommandLineParserBuilder CustomHelpArg(IArgument helpArg)
            {
                BuilderPropertyNullOrEmptyException<IArgument>.ThrowIfNullOrEmpty(nameof(helpArg), helpArg);
                _parser.HelpArg = helpArg;
                return this;
            }
            
            /// <summary>
            /// Gives the ability to allow custom formats
            /// </summary>
            /// <returns></returns>
            public CommandLineParserBuilder CustomValuesExtractor(IValuesExtractor valueExtractor)
            {
                _parser.Extractor = valueExtractor;
                return this;
            }

            /// <summary>
            /// Builds the <see cref="CommandLineParser"/>
            /// </summary>
            /// <returns></returns>
            /// <exception cref="BuilderPropertyNullOrEmptyException{T}"></exception>
            public CommandLineParser Build()
            {
                _parser.Extractor??= new StandardValuesExtractor();
                BuilderPropertyNullOrEmptyException<HashSet<IArgument>>.ThrowIfNullOrEmpty(nameof(_parser._arguments), _parser._arguments);
                return _parser;
            }
        }
    }
}
