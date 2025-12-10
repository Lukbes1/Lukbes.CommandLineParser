namespace Lukbes.CommandLineParser;

public class ParsingException(List<string> errors, string[] args) : Exception(CreateMessage(errors, args))
{

    public static string CreateMessage(List<string> errors, string[] args)
    {
        return $"Parsing '{args}' did not work: {FormatErrors(errors)}";
    }
    
    private static string FormatErrors(List<string> errors)
    {
        return string.Join(
            "", 
            errors.Select((e, i) => $"Error {i + 1}: {e}\n")
        );
    }
}