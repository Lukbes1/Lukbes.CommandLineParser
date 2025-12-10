using System.Runtime.CompilerServices;
using System.Text;
using Lukbes.CommandLineParser.Arguments;
using Lukbes.CommandLineParser.Extracting;

namespace Lukbes.CommandLineParser
{
    public class CommandLineParser
    {
        public static bool WithExceptions;

        private readonly HashSet<IArgument> _arguments = new();

        public IValuesExtractor? Extractor { get; private set; }

        public Dictionary<Delegate, (IArgument[] Arguments, bool Allrequired)> ArgumentHandlers { get; private set; } = [];
        
        public IArgument? HelpArg { get; private set; }
        
        private static readonly IArgument DEFAULT_HELP_ARG = Argument<bool>.Builder().Identifier(new("h", "help")).Description("Use to print help out").Build();
        
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
            ArgumentHandlers = other.ArgumentHandlers;
        }
        
        /// <summary>
        /// Main entry of Parsing. Parses the <paramref name="args"/>, fills all internal arguments with values and checks rules and dependencies. <br/>
        /// Calls every provided handler if conditions met.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="ParsingException"></exception>
        /// <exception cref="CommandLineArgumentException"></exception>
        public async Task<List<string>> ParseAsync(string[] args)
        {
            List<string> errors = new();
            var extractedValues = Extractor!.Extract(args); 
            errors.AddRange(extractedValues.errors);
            var hasHelpArg = extractedValues.identifierAndValues.Any(a => a.Key.Equals(HelpArg!.Identifier));
            if (hasHelpArg) //Stop early on Help:
            {
                PrintHelp();
                return errors;
            }
            ApplyValuesAndRules(extractedValues.identifierAndValues, errors);
            ValidateDependencies(errors);
            if (errors.Count > 0)
            {
                return errors;
            }
            foreach (var handlerEntry in ArgumentHandlers)
            {
                try
                {
                    if (handlerEntry.Value.Arguments.Length == 0)
                    {
                        handlerEntry.Key.DynamicInvoke();
                    }
                    else
                    {
                        if (handlerEntry.Value.Allrequired && !handlerEntry.Value.Arguments.All(x => x.HasValue))
                        {
                            continue;
                        }
                        object?[] values = handlerEntry.Value.Arguments.Select(a => a.Value).ToArray();
                        object? result = handlerEntry.Key.DynamicInvoke(values);
                        if (result is Task task)
                        {
                            await task; 
                        }
                    }
                }
                catch (Exception e)
                {
                    errors.Add(e.Message);
                    if (WithExceptions)
                    {
                        throw new ParsingException(errors, args);
                    }
                }
            }
            return errors;
        }
    
        private void ApplyValuesAndRules(
            Dictionary<ArgumentIdentifier, string?> extractedValues, List<string> errors)
        {
            foreach (var providedArg in extractedValues.Keys) //Checks if provided arguments are defined
            {
                if (!_arguments.Any(a => a.Identifier.Equals(providedArg)))
                {
                    if (WithExceptions)
                    {
                        throw new CommandLineArgumentDoesNotExistException(providedArg);
                    }
                    errors.Add(CommandLineArgumentDoesNotExistException.CreateMessage(providedArg));
                }
            }
            foreach (var argument in _arguments)
            {
                var foundArgumentPair = extractedValues.FirstOrDefault(x => x.Key.Equals(argument.Identifier));
                var applyErrors = argument.Apply(foundArgumentPair.Value);
                errors.AddRange(applyErrors);
            }
        }
        
        private void ValidateDependencies(List<string> errors)
        {
            foreach (var argument in _arguments) 
            {
                var allOtherArgs = _arguments.Where(a => !ReferenceEquals(a, argument)).ToHashSet();
                var dependencyErrors = argument.ValidateDependencies(allOtherArgs);
                errors.AddRange(dependencyErrors);
            }
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
                return $"Argument '{identifier}' could not be found.";
            }
            try
            {
                Type[] genericArgs = arg.GetType().GenericTypeArguments;
            
                if (genericArgs.Length is 0 or > 1)
                {
                    return $"Argument '{identifier}' must have only one generic argument.";
                }

                if (genericArgs[0] != typeof(T))
                {
                    return
                        $"Argument '{identifier}' must have the type  '{typeof(T)}'. Actual: '{genericArgs[0]}'";
                }
                
                Argument<T> castedArg = (Argument<T>)arg;
                argument = castedArg;
                return null;  
            }
            catch (Exception e)
            {
                return $"Argument '{identifier}' could not be casted. Actual: {e.Message}";
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
        
        /// <summary>
        /// Get the help string for this parser 
        /// </summary>
        /// <returns></returns>
        public string GetHelpString()
        {
            var result = new StringBuilder();
            result.AppendLine("Legend: ");
            result.AppendLine("\tOptional args -> [-shortIdentifier, --longIdentifier] : Datatype | Description (Default: defaultVal)");
            result.AppendLine("\tRequired args -> -shortIdentifier, --longIdentifier   : Datatype | Description (Default: defaultVal)");
            result.AppendLine("Arguments: ");
            foreach (var arg in _arguments)
            {
                result.Append('\t').Append(arg).AppendLine();
            }
            return result.Append('\t').Append(HelpArg).ToString();
        }
        
        private void PrintHelp()
        {
            string help = GetHelpString();
            Console.WriteLine(help);
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
                    if (_parser._arguments.Any(a => a.Identifier.Equals(argument.Identifier)))
                    {
                        throw new CommandLineArgumentUniqueException(argument);
                    }
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
            /// Gives the ability to allow custom formats instead of the usual --arg="" etc. However Help formatting wont change
            /// </summary>
            /// <returns></returns>
            public CommandLineParserBuilder CustomValuesExtractor(IValuesExtractor valueExtractor)
            {
                _parser.Extractor = valueExtractor;
                return this;
            }

            /// <summary>
            /// Add a handler that gets called if the specified combo of <paramref name="arguments"/> is provided. <br/>
            /// Gets called if and only if every given argument in <paramref name="arguments"/> has a value <br/>
            /// If <paramref name="arguments"/> is empty, the handler will always be called after parsing
            /// </summary>
            /// <param name="handler">The function that's called if the combo of arguments is provided</param>
            /// <param name="allRequired">If all args should have values. If true and any arg has no value, handler wont be called</param>
            /// <param name="arguments">the arguments that should be provided</param>
            /// <example>
            /// The following code takes in 3 Arguments and is only called if Url-, audio- and videoArgument HasValue returns true 
            /// <code>
            /// builder.Handler((string url, bool audio, bool video) =>
            /// {
            ///      Console.WriteLine($"Url: {url}");
            ///     Console.WriteLine($"Audio: {audio}");
            ///     Console.WriteLine($"Video: {video}");
            /// }, urlArgument,  audioArgument, videoArgument);
            /// </code>
            /// </example>
            /// <returns></returns>
            /// <exception cref="InvalidOperationException"></exception>
            public CommandLineParserBuilder Handler(Delegate handler, bool allRequired = false, params IArgument[] arguments)
            {
                var parameters = handler.Method.GetParameters();
                
                if (parameters.Length != arguments.Length)
                {
                    throw new ArgumentException($"The number of arguments in the handler does not match the number of arguments provided. Expected: {parameters.Length}, Actual: {arguments.Length}");
                }
                
                for (int i = 0; i < parameters.Length; i++)
                {
                    var expected = parameters[i].ParameterType;
                    var actual = arguments[i].ValueType;
                    var argument = arguments[i];

                    if (!IsCompatible(expected, actual))
                    {
                        throw new ArgumentException(
                            $"The type of {argument.Identifier} does not match expected {expected} type. Actual: {actual}");
                    }
                }
                
                _parser.ArgumentHandlers.Add(handler, (arguments, allRequired));
                return this;
            }
            
            private static Type UnwrapNullable(Type type)
            {
                return Nullable.GetUnderlyingType(type) ?? type;
            }
            
            private static bool IsCompatible(Type expected, Type actual)
            {
                expected = UnwrapNullable(expected);
                actual = UnwrapNullable(actual);

                return expected == actual;
            }

            /// <summary>
            /// Builds the <see cref="CommandLineParser"/>
            /// </summary>
            /// <returns></returns>
            /// <exception cref="BuilderPropertyNullOrEmptyException{T}"></exception>
            public CommandLineParser Build()
            {
                _parser.Extractor ??= new StandardValuesExtractor();
                BuilderPropertyNullOrEmptyException<HashSet<IArgument>>.ThrowIfNullOrEmpty(nameof(_parser._arguments), _parser._arguments);
                _parser.HelpArg ??= DEFAULT_HELP_ARG;
                return _parser;
            }
        }
    }
}
