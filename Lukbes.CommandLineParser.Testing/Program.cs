
using Lukbes.CommandLineParser.Arguments;
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
            CommandLineParser.WithExceptions = false;
            var audioArgument = new Argument<bool>(builder =>
            {
                builder.Identifier(new("a", "audio"));
                builder.DefaultValue(true);
                return builder.Build();
            });
            
            
            var videoArgument = new Argument<bool>(builder =>
            {
                builder.ShortIdentifier("v");
                builder.DefaultValue(true);
                builder.LongIdentifier("video");
                return builder.Build();
            });
                
            var urlArgument = new Argument<string>(builder =>
            {
                builder.LongIdentifier("Url");
                builder.IsRequired();
                builder.Description("The youtube link");
                builder.Rule(new HttpLinkRule());
                return builder.Build();
            });

            CommandLineParser youtubeParser = new CommandLineParser(builder =>
            {
                builder.Arguments(urlArgument, videoArgument, audioArgument);
                builder.Handler((string url, bool audio, bool video) =>
                {
                    Console.WriteLine($"Url: {url}");
                    Console.WriteLine($"Audio: {audio}");
                    Console.WriteLine($"Video: {video}");
                }, urlArgument,  audioArgument, videoArgument);
                return builder.Build();
            });
            var errors = youtubeParser.Parse(args);

            foreach (var error in errors)
            {
                Console.WriteLine(error);
            }
        }
    }
}
