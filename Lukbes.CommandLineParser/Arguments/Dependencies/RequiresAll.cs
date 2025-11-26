namespace Lukbes.CommandLineParser.Arguments.Dependencies;

/// <summary>
/// If the <see cref="Argument{T}"/> it is defined on has no value, nothing happens. <br/>
/// Otherwise, every <see cref="Argument{T}"/> has to be present and has to have a value.
/// </summary>
public class RequiresAll : IDependency
{
    private readonly HashSet<ArgumentIdentifier> _requiresArgs;
    
    public RequiresAll(HashSet<IArgument> requiresArgs)
    {
        _requiresArgs = requiresArgs.Select(a => a.Identifier).ToHashSet();
    }

    public RequiresAll(IArgument requires)
    {
        _requiresArgs = [requires.Identifier];
    }

    public RequiresAll(ArgumentIdentifier requires)
    {
        _requiresArgs = [requires];
    }

    public RequiresAll(HashSet<ArgumentIdentifier> requiresArgs)
    {
        _requiresArgs = new(requiresArgs);
    }

    public RequiresAll(params ArgumentIdentifier[] requiresArg)
    {
        _requiresArgs = new(requiresArg);
    }
    
    public RequiresAll(params IArgument[] requiresArgs)
    {
        _requiresArgs =  requiresArgs.Select(a => a.Identifier).ToHashSet();
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
            if (otherArgs.FirstOrDefault(a => a.Identifier.Equals(requiredArg)) is null)
            {
                 string errorMessage =
                    $"Error: {argument} requires {requiredArg}. actual: {requiredArg} was missing or has no value";
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