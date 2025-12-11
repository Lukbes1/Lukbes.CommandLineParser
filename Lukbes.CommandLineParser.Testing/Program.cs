
using System.Net.NetworkInformation;
using Lukbes.CommandLineParser.Arguments;
using Lukbes.CommandLineParser.Arguments.Dependencies;
using Lukbes.CommandLineParser.Arguments.Rules;
using Lukbes.CommandLineParser.Arguments.TypeConverter;

namespace Lukbes.CommandLineParser.Testing
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                await PrintName();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
         
        }

        static async Task List_example(string[] args)
        {
            CommandLineParser.WithExceptions = true;
            var numbersArg = Argument<List<int>>.Builder().LongIdentifier("numbers").Build();
            
            var parser = CommandLineParser.Builder().Argument(numbersArg).Build();
            await parser.ParseAsync(args);
            foreach (var num in numbersArg.Value!)
            {
                Console.WriteLine(num);
            }
        }
        
        static async Task Example_PrintName(string[] args)
        {
            var nameArg = Argument<string>.Builder()
                .DefaultValue("tom")
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


        private readonly struct CustomPoint
        {
            public readonly int X;
            public readonly int Y;

            public CustomPoint(int x, int y) => (X,Y) = (x, y);

            public override string ToString()
            {
                return "{" + X + ", " + Y + "}";
            }
        }
        
         private class NonNegativePoints : IRule<List<CustomPoint>>
         {
              public string? Validate(Argument<List<CustomPoint>> argument)
              {
                  if (argument.Value!.Any(p => p.X < 0 || p.Y < 0))
                  {
                      return $"X and Y cannot be negative";
                  }
                  return null;
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
                var values = value.Split(";");
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
            CommandLineParser.WithExceptions = false;
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
                .Description("The coordinate of the point")
                .Converter(new CustomPointConverter()) //<- The custom converter
                .Build();

            var parser = CommandLineParser.Builder().Arguments(pointArg).Build();
            var errors = await parser.ParseAsync(args);
            foreach (var error in errors)
            {
                Console.WriteLine(error);
            }
            Console.WriteLine("The Point: " + pointArg.Value);
            
            //Its also possible to add a converter to the Known default converters. Helpful, if you want multiple args of the same custom type:
            bool success = DefaultConverterFactory.TryAdd(new CustomPointConverter());
            foreach (var type in DefaultConverterFactory.Types) //Demonstrating that the type is actually registered (and the others)
            {
                Console.WriteLine(type.Name);
            }
            
            var otherArg = Argument<CustomPoint>.Builder()
                .Identifier(new("p", "Point"))
                .IsRequired()
                .Description("The coordinate of the block")
                .Build(); //No custom converter needed anymore
            
            var otherParser = CommandLineParser.Builder().Arguments(otherArg).Build();
            var otherErrors = await otherParser.ParseAsync(args);
            if (otherErrors.Count == 0)
            {
                Console.Write("Worked! ");
                Console.WriteLine(otherArg.Value);
            }
        }
        
  
        static async Task CustomPointListExample()
        {
            DefaultConverterFactory.TryAddList<CustomPoint>(new CustomPointConverter());
            
            var points = Argument<List<CustomPoint>>.Builder()
                .Identifier(new("p", "Points"))
                .IsRequired()
                .Rule(new ListCountBetweenXAndY<CustomPoint>(1, 3))
                .Rule(new ListRule<CustomPoint>(p => p.X > 0 && p.Y > 0))
                .Description("The coordinate of the block")
                .Build();
            
            var otherParser = CommandLineParser.Builder().Argument(points).Build();
            string[] args = ["-p=10;30,20;40,30;-5,109;2"];
            var otherErrors = await otherParser.ParseAsync(args);
            if (otherErrors.Count == 0)
            {
                Console.WriteLine("Worked! ");
                foreach (var point in points.Value)
                {
                    Console.WriteLine(point.ToString());
                }
            }
            else
            {
                foreach (var error in otherErrors)
                {
                    Console.WriteLine(error);
                }
            }
        }

        public static async Task PrintName()
        {
            string[] args = [];

            var nameArg = Argument<string>.Builder()
                .LongIdentifier("name")
                .Build();
            
            var parser = CommandLineParser.Builder()
                .Argument(nameArg)
                .Handler((string name) =>
                {
                    Console.WriteLine("Your name is: " + name);
                }, nameArg) // Will always be called now
                .Handler(() =>
                {
                    Console.WriteLine("I got called!"); 
                })
                .Build();

            await parser.ParseAsync(args);
        }
        

        public static async Task BindingArgs(string[] args)
        {
            CommandLineParser.WithExceptions = true; //This example fails early
            var firstNameArg =  Argument<string>.Builder()
                .Description("The first Name")
                .ShortIdentifier("f")
                .Build();
            
            var ageArg = Argument<int>.Builder()
                .ShortIdentifier("a")
                .Build();
            
            var lastNameArg = Argument<string>.Builder()
                .Description("The Last Name")
                .Dependency(new NotDependency(ageArg.Identifier))
                .ShortIdentifier("l")
                .Build();

            var parser = CommandLineParser.Builder()
                .Arguments(firstNameArg, ageArg, lastNameArg)
                .Build();

            try
            {
                await parser.ParseAsync(args);
                Console.WriteLine(firstNameArg.Value);
                Console.WriteLine(lastNameArg.Value);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }
        
        private class NotDependency : IDependency
        {
            private readonly ArgumentIdentifier _notArg;

            public NotDependency(ArgumentIdentifier notArg)
            {
                _notArg = notArg;
            }
            public List<string> Check(IArgument argument, HashSet<IArgument> otherArgs)
            {
                if (!argument.HasValue)
                {
                    return [];
                }
                
                List<string> result = [];
                var foundArg = otherArgs.FirstOrDefault(a => a.Identifier.Equals(_notArg));
                if (foundArg is not null && foundArg.HasValue)
                {
                    string errorMessage =
                        $"'{argument.Identifier}' requires '{_notArg}' to not be present";
                    if (CommandLineParser.WithExceptions)
                    {
                        throw new DependencyException(errorMessage);
                    }
                    result.Add(errorMessage);
                }
                return result;
            }
        }
    }
}
