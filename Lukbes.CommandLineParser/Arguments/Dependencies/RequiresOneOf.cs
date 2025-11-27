namespace Lukbes.CommandLineParser.Arguments.Dependencies;

/// <summary>
/// If used, checks if the <see cref="Argument{T}"/> it is defined on has a value and then, if true, checks if at least one of the _requiresArgs is provided and has a Value 
/// </summary>
public class RequiresOneOf : IDependency
{
    private readonly HashSet<ArgumentIdentifier> _requiresArgs;
    
    /// <summary>
    ///
    /// </summary>
    /// <param name="requiresArgs">The arguments of which at least one must be present and has a value</param>
    public RequiresOneOf(HashSet<IArgument> requiresArgs)
    {
        if (requiresArgs.Count == 0)
        {
            throw new ArgumentException("You must specify at least one requires");
        }
        _requiresArgs = requiresArgs.Select(a => a.Identifier).ToHashSet();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="requires">The argument that must be present and has to have a value</param>
    public RequiresOneOf(IArgument requires)
    {
        _requiresArgs = [requires.Identifier];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="requires">The identifier of the argument that must be present and has to have a value</param>
    public RequiresOneOf(ArgumentIdentifier requires)
    {
        _requiresArgs = [requires];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="requiresArgs">The identifier of the arguments of which at least one must be present and has a value</param>
    public RequiresOneOf(HashSet<ArgumentIdentifier> requiresArgs)
    {
        if (requiresArgs.Count == 0)
        {
            throw new ArgumentException("You must specify at least one requires");
        }
        _requiresArgs = new(requiresArgs);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="first">the first argument of many</param>
    /// <param name="requiresArgs">The identifier of the arguments of which at least one must be present and has a value</param>
    public RequiresOneOf(ArgumentIdentifier first, params ArgumentIdentifier[] requiresArgs)
    {
        _requiresArgs =
        [
            ..requiresArgs,
            first
        ];
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="first">the first argument of many</param>
    /// <param name="requiresArgs">The arguments of which at least one must be present and has a value</param>
    public RequiresOneOf(IArgument first, params IArgument[] requiresArgs)
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
        
        foreach (var arg in otherArgs)
        {
            if (_requiresArgs.Contains(arg.Identifier) && arg.HasValue)
            {
                return [];
            }
        }
        string requiredArgs = "[" + string.Join(";", _requiresArgs) + "]";

        string errorMessage =
            $"Error: \"{argument}\" requires any of the following \"{requiredArgs}\"";
        if (CommandLineParser.WithExceptions)
        {
            throw new DependencyException(errorMessage);
        } 
        return [errorMessage];
    }
}