using System.Runtime.Serialization;

namespace Lukbes.CommandLineParser.Arguments;

/// <summary>
/// Gets thrown if an error occurs during Argument construction, values extraction or whilest trying to apply the provided values onto the arguments 
/// </summary>
public class CommandLineArgumentException : Exception
{
    public CommandLineArgumentException()
    {
    }

    protected CommandLineArgumentException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public CommandLineArgumentException(string? message) : base(message)
    {
    }

    public CommandLineArgumentException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}