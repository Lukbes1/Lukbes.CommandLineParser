namespace Lukbes.CommandLineParser.Arguments.Dependencies;

public class RequiresOneOf : IDependency
{
    private readonly HashSet<ArgumentIdentifier> _requiresArgs;
    
    public RequiresOneOf(HashSet<IArgument> requiresArgs)
    {
        _requiresArgs = requiresArgs.Select(a => a.Identifier).ToHashSet();
    }

    public RequiresOneOf(IArgument requires)
    {
        _requiresArgs = [requires.Identifier];
    }

    public RequiresOneOf(ArgumentIdentifier requires)
    {
        _requiresArgs = [requires];
    }

    public RequiresOneOf(HashSet<ArgumentIdentifier> requiresArgs)
    {
        _requiresArgs = new(requiresArgs);
    }

    public RequiresOneOf(params ArgumentIdentifier[] requiresArgs)
    {
        _requiresArgs = new(requiresArgs);
    }
    
    public RequiresOneOf(params IArgument[] requiresArgs)
    {
        _requiresArgs =  requiresArgs.Select(a => a.Identifier).ToHashSet();
    }
    
    public List<string> Check(IArgument argument, HashSet<IArgument> otherArgs)
    {
        if (!argument.HasValue)
        {
            return [];
        }
        
        bool anyPresent = false;
        foreach (var arg in otherArgs)
        {
            if (_requiresArgs.Contains(arg.Identifier) && arg.HasValue)
            {
                return [];
            }
        }
        string requiredArgs = "[" + string.Join(";", _requiresArgs) + "]";

        string errorMessage =
            $"Error: {argument} requires any of the following {requiredArgs}";
        if (CommandLineParser.WithExceptions)
        {
            throw new DependencyException(errorMessage);
        } 
        return [errorMessage];
    }
}