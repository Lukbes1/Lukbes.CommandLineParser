namespace Lukbes.CommandLineParser.Arguments;

/// <summary>
/// An <see cref="ArgumentIdentifier"/> is uniquely defined by either their <see cref="ShortIdentifier"/> OR their <see cref="LongIdentifier"/>
/// </summary>
/// <param name="shortIdentifier"></param>
/// <param name="longIdentifier"></param>
public class ArgumentIdentifier(string? shortIdentifier = null, string? longIdentifier = null)
{
    public string? ShortIdentifier = shortIdentifier;
    public string? LongIdentifier = longIdentifier;

    public bool Validate()
    {
        return ShortIdentifier is not null || LongIdentifier is not null;
    }

    public static implicit operator ArgumentIdentifier((string? shortIdentifier, string? longIdentifier) args)
    {
        return new ArgumentIdentifier(args.shortIdentifier, args.longIdentifier);
    }

    public override string ToString()
    {
        if (ShortIdentifier is not null && LongIdentifier is null)
        {
            return $"-{ShortIdentifier}";    
        }
        if (ShortIdentifier is null && LongIdentifier is not null)
        {
            return $"--{LongIdentifier}"; 
        } 

        if (shortIdentifier is null && longIdentifier is null)
        {
            return "";
        }
        return $"-{ShortIdentifier}, --{LongIdentifier}";
    }
    
    protected bool Equals(ArgumentIdentifier other)
    {
        return ShortIdentifier == other.ShortIdentifier || LongIdentifier == other.LongIdentifier;
    }
    
    protected bool Equals(string other)
    {
        return ShortIdentifier == other || LongIdentifier == other;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() == GetType())
        {
            return Equals((ArgumentIdentifier)obj);
        }
        if (obj.GetType() == typeof(string))
        {
            return Equals((string)obj);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ShortIdentifier, LongIdentifier);
    }
}