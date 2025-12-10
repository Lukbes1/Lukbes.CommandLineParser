namespace Lukbes.CommandLineParser.Arguments.Dependencies;

/// <summary>
///  If used, checks if the <see cref="Argument{T}"/> it is defined on has a value and then, if true, checks if all the _requiresArgs are provided and have a Value 
/// </summary>
public sealed class RequiresAll : IDependency
{
    private readonly HashSet<ArgumentIdentifier> _requiresArgs;
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="requiresArgs">The arguments of which all must be present and have a value</param>
    public RequiresAll(HashSet<IArgument> requiresArgs) 
    {
        if (requiresArgs.Count == 0)
        {
            throw new CommandLineArgumentException("You must specify at least one requires");
        }
        _requiresArgs = requiresArgs.Select(a => a.Identifier).ToHashSet();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="requires">The argument which must be present and have a value</param>
    public RequiresAll(IArgument requires)
    {
        _requiresArgs = [requires.Identifier];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="requires">The identifier of the argument which must be present and have a value</param>
    public RequiresAll(ArgumentIdentifier requires)
    {
        _requiresArgs = [requires];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="requiresArgs">The identifier of the arguments of which all must be present and have a value</param>
    /// <exception cref="CommandLineArgumentException">if requiresArgs is empty</exception>
    public RequiresAll(HashSet<ArgumentIdentifier> requiresArgs)
    {
        if (requiresArgs.Count == 0)
        {
            throw new CommandLineArgumentException("You must specify at least one requires");
        }
        _requiresArgs = new HashSet<ArgumentIdentifier>(requiresArgs);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="first">the identifier of the first argument of many</param>
    /// <param name="requiresArg">The identifier of the arguments of which at least one must be present and has a value</param>
    public RequiresAll(ArgumentIdentifier first, params ArgumentIdentifier[] requiresArg)
    {
        _requiresArgs =
        [
            ..requiresArg,
            first
        ];
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="first">the first argument of many</param>
    /// <param name="requiresArgs">The arguments of which at least one must be present and has a value</param>
    public RequiresAll(IArgument first, params IArgument[] requiresArgs)
    {
        _requiresArgs = requiresArgs.Select(a => a.Identifier).ToHashSet();
        _requiresArgs.Add(first.Identifier);
    }
    
    public List<string> Check(IArgument argument, HashSet<IArgument> otherArgs)
    {
        if (!argument.HasValue)
        {
            return [];
        }

        List<string> result = [];
        foreach (var requiredArg in _requiresArgs)
        {
            var foundArg = otherArgs.FirstOrDefault(a => a.Identifier.Equals(requiredArg));
            if (foundArg is null || !foundArg.HasValue)
            {
                 string errorMessage =
                    $"'{argument}' requires '{requiredArg}'. actual: '{requiredArg}' was missing or has no value";
                if (CommandLineParser.WithExceptions)
                {
                    throw new DependencyException(errorMessage);
                }
                result.Add(errorMessage);
            }
        }
        return result;
    }
}