using System.Globalization;
using Lukbes.CommandLineParser.Arguments;
using Lukbes.CommandLineParser.Arguments.Dependencies;
using Lukbes.CommandLineParser.Arguments.Rules;
using Lukbes.CommandLineParser.Arguments.TypeConverter;

namespace Lukbes.CommandLineParser.Testing
{
    internal class Program
    {
        static void Main(string[] args)
        {
            testTwo(args);

        }

        private static void testOne(string[] args)
        {
            CommandLineParser.WithExceptions = true;
            CommandLineParser parser = new CommandLineParser(b =>
            {
                b.Argument(new Argument<string>(a =>
                {
                    a.Identifier(new("u", "Url"))
                        .Converter(new StringConverter());
                    return a.Build();
                }));
                return b.Build();
            });
            
            var errors = parser.Parse(args);
            foreach (var error in errors)
            {
                Console.WriteLine(error);
            }

            var argError = parser.TryGetArgument<string>(new(longIdentifier: "Url"), out var urlArg);
            Console.WriteLine(argError);
            Console.WriteLine(urlArg?.Value);
        }

        private static void testTwo(string[] args)
        {
            var audioArgument = new Argument<bool>(builder =>
            {
                builder.Identifier(new("a", "audio"));
                builder.Converter(new BoolConverter());
                return builder.Build();
            });
            
            var videoArgument = new Argument<bool>(builder =>
            {
                builder.Identifier(new("v", "video"));
                builder.Converter(new BoolConverter());
                return builder.Build();
            });
                
            var urlArgument = new Argument<string>(builder =>
            {
                builder.IsRequired();
                builder.Description("The youtube link");
                builder.LongIdentifier("Url");
                builder.Converter(new StringConverter());
                builder.Rule(new HttpLinkRule());
                builder.Dependency(new RequiresOneOf(audioArgument, videoArgument ));
                return builder.Build();
            });

            CommandLineParser youtubeParser = new CommandLineParser(builder =>
            {
                builder.Arguments(urlArgument, videoArgument, audioArgument);
                return builder.Build();
            });
            var errors = youtubeParser.Parse(args);

            Console.WriteLine(urlArgument.Value);
            Console.WriteLine(audioArgument.Value);
            Console.WriteLine(videoArgument.Value);

            foreach (var error in errors)
            {
                Console.WriteLine(error);
            }
        }
    }
}
