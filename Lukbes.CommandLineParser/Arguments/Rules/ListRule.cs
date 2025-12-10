namespace Lukbes.CommandLineParser.Arguments.Rules;

/// <summary>
/// A ListRule is applied on the children of a List to check whether a certain condition is true or not.
/// </summary>
/// <typeparam name="T">The type of the child Argument on which it can be applied</typeparam>
public class ListRule<T> : IRule<List<T>>
{
    private readonly Func<T, bool> _childrenPredicate;

    /// <summary>
    /// Set the rule checked for every child
    /// </summary>
    /// <param name="childrenPredicate">The predicate to check against in each child</param>
    public ListRule(Func<T, bool> childrenPredicate)
    {
        _childrenPredicate = childrenPredicate;
    }
    
    public string? Validate(Argument<List<T>> argument)
    {
        foreach (var value in argument.Value!)
        {
            var success  = _childrenPredicate.Invoke(value);
            if (!success)
            {
                return $"There was an error with '{argument.Identifier}'. '{value}' did not fulfill the rule '{GetType().GetFriendlyTypeName()}'";
            }
        }
        return null;
    }
}