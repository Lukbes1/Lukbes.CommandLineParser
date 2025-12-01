
using System.Net.NetworkInformation;
using Lukbes.CommandLineParser.Arguments;
using Lukbes.CommandLineParser.Arguments.Rules;
using Lukbes.CommandLineParser.Arguments.TypeConverter;

namespace Lukbes.CommandLineParser.Testing
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await Example_MoreComplex(args);

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
        private class IntBetweenXAndY : IRule<int>
        {
            private readonly int _lowerBound;
            private readonly int _upperBound;

            public IntBetweenXAndY(int lowerBound, int upperBound)
            {
                _lowerBound = lowerBound;
                _upperBound = upperBound;
            }
            public string? Validate(Argument<int> argument)
            {
                if (argument.Value < _lowerBound || argument.Value > _upperBound)
                {
                    return $"Value must be between {_lowerBound} and {_upperBound}";
                }
                return null;
            }
        }

        private class ListConverter<T>(IConverter<T> itemConverter) : IConverter<IList<T>>
        {
            private readonly IConverter<T> _itemConverter = itemConverter;

            public string? TryConvert(string? value, out IList<T>? result)
            {
                if (value is null)
                {
                    result = [];
                    return null;
                }
                IList<T> items = new List<T>();
                result = items;
                foreach (var item in value.Split(","))
                {
                    var convertError = _itemConverter.TryConvert(item, out T? itemResult);
                    if (convertError is null)
                    {
                        items.Add(itemResult);
                    }
                    else
                    {
                        return convertError;
                    }
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

            var intervalsArg = Argument<IList<int>>.Builder()
                .Identifier(new("i", "Intervals"))
                .Description("The intervals to wait between each reattempt in ms")
                .Converter(new ListConverter<int>(new IntConverter())) //Custom Converter for IList
                .Build();
            
            var reattemptTimesArg= Argument<int>.Builder()
                .Identifier(new("r", "ReattemptTimes"))
                .Description("The times the request should be reattempted")
                .RequiresAll(intervalsArg)
                .Rule(new IntBetweenXAndY(0,3)) //Custom Boundary Checker
                .Build();
            
            var parser = CommandLineParser.Builder()
                .Arguments(urlArg, reattemptTimesArg, intervalsArg)
                .Handler(async (string url, int reattemptTimes, IList<int>? intervals) =>
                {
                    int count = 0;
                    int maxRetries = (reattemptTimes == 0 ? intervals?.Count : 1) ?? 1;
                    using var ping = new Ping();

                    PingReply? reply;
                    do
                    {
                        try
                        {
                            reply = await ping.SendPingAsync(url, 1000); // 1 sec timeout
                            if (reply.Status == IPStatus.Success)
                            {
                                Console.WriteLine($"Success: {reply.RoundtripTime}ms");
                                break; // exit loop
                            }
                          
                        }
                        catch (Exception e)
                        {
                        }
                        Console.WriteLine($"Failed");
                        
                        // Wait if intervals are provided
                        if (intervals is not null && intervals.Count > 0 && count < intervals.Count)
                        {
                            var waitTime = intervals[count];
                            Console.WriteLine($"Waiting for {waitTime}ms");
                            await Task.Delay(waitTime);
                        }
                        if (count < maxRetries - 1)
                        {
                            Console.WriteLine("Retrying...");
                        }
                        count++;
                    } while (count < maxRetries);
                }, allRequired:false, urlArg, reattemptTimesArg, intervalsArg)
                .Build();

            var errors = await parser.ParseAsync(args);
            foreach (var error in errors)
            {
                Console.WriteLine(error);
            }
        }
        
        
    }
}
