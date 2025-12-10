namespace Lukbes.CommandLineParser.Arguments.Dependencies;

/// <summary>
/// Use <see cref="OnlyWith"/>, if you want to bind one argument to exist only with another <br/>
/// This has the following implications: <br/>
/// 1. This exists, others dont ->  fails
/// 2. This doesnt exist, others exist -> fails
/// 3. This exists, others exist -> succeeds
/// </summary>
public class OnlyWith : IDependency
{
     private readonly HashSet<ArgumentIdentifier> _boundArgs;
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="boundArgs">The arguments of which all must be present and have a value</param>
    public OnlyWith(HashSet<IArgument> boundArgs) 
    {
        if (boundArgs.Count == 0)
        {
            throw new CommandLineArgumentException("You must specify at least one requires");
        }
        _boundArgs = boundArgs.Select(a => a.Identifier).ToHashSet();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="boundArg">The argument which must be present and have a value</param>
    public OnlyWith(IArgument boundArg)
    {
        _boundArgs = [boundArg.Identifier];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="boundArg">The identifier of the argument which must be present and have a value</param>
    public OnlyWith(ArgumentIdentifier boundArg)
    {
        _boundArgs = [boundArg];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="boundArgs">The identifier of the arguments of which all must be present and have a value</param>
    /// <exception cref="CommandLineArgumentException">if requiresArgs is empty</exception>
    public OnlyWith(HashSet<ArgumentIdentifier> boundArgs)
    {
        if (boundArgs.Count == 0)
        {
            throw new CommandLineArgumentException("You must specify at least one requires");
        }
        _boundArgs = new HashSet<ArgumentIdentifier>(boundArgs);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="first">the identifier of the first argument of many</param>
    /// <param name="boundArgs">The identifier of the arguments of which at least one must be present and has a value</param>
    public OnlyWith(ArgumentIdentifier first, params ArgumentIdentifier[] boundArgs)
    {
        _boundArgs =
        [
            ..boundArgs,
            first
        ];
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="first">the first argument of many</param>
    /// <param name="boundArgs">The arguments of which at least one must be present and has a value</param>
    public OnlyWith(IArgument first, params IArgument[] boundArgs)
    {
        _boundArgs = boundArgs.Select(a => a.Identifier).ToHashSet();
        _boundArgs.Add(first.Identifier);
    }
    
    public List<string> Check(IArgument argument, HashSet<IArgument> otherArgs)
    {
        List<string> result = [];
        List<ArgumentIdentifier> foundBoundArgs = [];
        foreach (var boundArg in _boundArgs)
        {
            var foundArg = otherArgs.FirstOrDefault(a => a.Identifier.Equals(boundArg));
            if ((foundArg is null || !foundArg.HasValue) && argument.HasValue)
            {
                string errorMessage =
                    $"'{argument.Identifier}' is bound and thus requires '{boundArg}'. Actual: '{boundArg}' was missing or has no value";
                if (CommandLineParser.WithExceptions)
                {
                    throw new DependencyException(errorMessage);
                }
                result.Add(errorMessage);
            }
            else if (foundArg is not null)
            {
                foundBoundArgs.Add(foundArg.Identifier);
            }
        }

        if (argument.HasValue || foundBoundArgs.Count == 0) return result;
        string args =  string.Join(", ", foundBoundArgs.Select(x => x.ToString()));
        string error = $"'{argument.Identifier}' is bound to the argument '{args}' but '{argument.Identifier}' was not present";
        if (CommandLineParser.WithExceptions)
        {
            throw new DependencyException(error);
        }
        return [error];
    }
}