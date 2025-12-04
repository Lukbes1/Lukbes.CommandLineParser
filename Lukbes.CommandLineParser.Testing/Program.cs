
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using Lukbes.CommandLineParser.Arguments;
using Lukbes.CommandLineParser.Arguments.Rules;
using Lukbes.CommandLineParser.Arguments.TypeConverter;

namespace Lukbes.CommandLineParser.Testing
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await BindingArgs(args);
        }

        static async Task Example_PrintName(string[] args)
        {
            var nameArg = Argument<string>.Builder()
                .Identifier(new("n", "name"))
                .Build();
            
            var parser = CommandLineParser.Builder()
                .Argument(nameArg)
                .Handler((string name) => Console.WriteLine($"Hello {name}"), allRequired:true, nameArg)
                .Build();

            var errors = await parser.ParseAsync(args);
            foreach (var error in errors)
            {
                Console.WriteLine(error);
            }
            
            //Retrieve nameArg when you have no access to the original Arg:
            var argError = parser.TryGetArgument<string>(new("n", "name"), out  var nameArgRetrieved);
            if (argError is null && !nameArgRetrieved!.HasValue)
            {
                Console.WriteLine("Hello from Mr.Unknown");
            }
        }

        /// <summary>
        /// Custom Rule for boundary checks
        /// </summary>
        private class ListCountBetweenXAndY<T> : IRule<List<T>>
        {
            private readonly int _lowerBound;
            private readonly int _upperBound;

            public ListCountBetweenXAndY(int lowerBound, int upperBound)
            {
                _lowerBound = lowerBound;
                _upperBound = upperBound;
            }

            public string? Validate(Argument<List<T>> argument)
            {
                if (argument.Value!.Count < _lowerBound || argument.Value.Count > _upperBound)
                {
                    return $"Count of list must be between {_lowerBound} and {_upperBound}";
                }

                return null;
            }
        }
        
        static async Task Example_MoreComplex(string[] args)
        {
            var urlArg = Argument<string>.Builder()
                .Identifier(new("u", "Url"))
                .IsRequired()
                .Build();

            var intervalsArg = Argument<List<int>>.Builder()
                .Identifier(new("i", "Intervals"))
                .Description("The intervals to wait between each reattempt in ms")
                .Rule(new ListCountBetweenXAndY<int>(0,3)) //new Rule
                .Build();
            
            var parser = CommandLineParser.Builder()
                .Arguments(urlArg, intervalsArg)
                .Handler(PingWithRetriesAsync, allRequired:false, urlArg, intervalsArg)
                .Build();

            var errors = await parser.ParseAsync(args);
            foreach (var error in errors)
            {
                Console.WriteLine(error);
            }
        }
        
        public static async Task PingWithRetriesAsync(string url, List<int>? intervals)
        {
            int maxTries = intervals?.Count ?? 1; // if no intervals -> single try
            using var ping = new Ping();

            for (int attempt = 1; attempt <= maxTries; attempt++)
            {
                int index = attempt - 1;
                int timeout = intervals != null ? intervals[index] : 1000;

                Console.WriteLine($"Attempt {attempt}/{maxTries}: sending ping (timeout {timeout}ms)");

                try
                {
                    PingReply reply = await ping.SendPingAsync(url, timeout);

                    if (reply.Status == IPStatus.Success)
                    {
                        Console.WriteLine($"Success: {reply.RoundtripTime}ms");
                        return;
                    }
                     Console.WriteLine($"Attempt {attempt} failed: {reply.Status}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ping error on attempt {attempt}: {ex.Message}");
                }

                // If more tries remain, wait; otherwise report final failure
                if (attempt < maxTries)
                {
                    int delay = intervals![index];
                    Console.WriteLine($"Retry {attempt}/{maxTries}: waiting {delay}ms before next try...");
                    await Task.Delay(delay);
                }
                else
                {
                    Console.WriteLine("Failed all retries.");
                }
            }
        }


        private readonly struct CustomPoint(int X, int Y)
        {
            public override string ToString()
            {
                return "{" + X + ", " + Y + "}"; 
            }
        }

        private class CustomPointConverter : IConverter<CustomPoint>
        {
            public string? TryConvert(string? value, out CustomPoint result)
            {
                result = default;
                if (string.IsNullOrEmpty(value))
                {
                    return "Point must not be null";
                }

                value = value.Trim();
                var values = value.Split(",");
                if (values.Length != 2)
                {
                    return "There must be exactly two values";
                }
                bool successX = int.TryParse(values[0], out var x);
                if (!successX)
                {
                    return "X value was not valid! Actual: " + values[0];
                }
                
                bool successY = int.TryParse(values[1], out var y);
                if (!successY)
                {
                    return "Y value was not valid! Actual: " + values[1];
                }

                result = new CustomPoint(x, y);
                return null;
            }
        }
        
        public static async Task Custom_type_example(string[] args)
        {
            /* var pointArg = Argument<CustomPoint>.Builder()
                .Identifier(new("p", "Point"))
                .Description("The coordinate of the block")
                .Build();
                Would not work, because you have to specify a converter for this type
                Certain types are predefined, see DefaultConverterFactory.GetTypes() 
            */
            
            var pointArg = Argument<CustomPoint>.Builder()
                .Identifier(new("p", "Point"))
                .IsRequired()
                .Description("The coordinate of the block")
                .Converter(new CustomPointConverter())
                .Build();

            var parser = CommandLineParser.Builder().Arguments(pointArg).Build();
            var errors = await parser.ParseAsync(args);
            foreach (var error in errors)
            {
                Console.WriteLine(error);
            }

            Console.WriteLine("The Point: " + pointArg.Value);
        }


        public static async Task BindingArgs(string[] args)
        {
            CommandLineParser.WithExceptions = true;
            var firstNameArg =  Argument<string>.Builder()
                .Description("The first Name")
                .ShortIdentifier("f")
                .Build();
            
            var lastName = Argument<string>.Builder()
                .Description("The Last Name")
                .OnlyWith(firstNameArg)
                .ShortIdentifier("l")
                .Build();

            var parser = CommandLineParser.Builder()
                .Arguments(firstNameArg, lastName)
                .Build();

            try
            {
                await parser.ParseAsync(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
          

        }
        
    }
}
