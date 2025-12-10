# 🚀 Lukbes.CommandLineParser

A CommandlineParser used to define command line arguments for projects of small but also bigger sizes.

---

## ✨ Features

- Create custom arguments
- Use custom rules for your arguments
- Define custom types and converters for your arguments
- Create dependencies between multiple arguments 
- Define how the argument's format should be
- Builder pattern for creation of arguments and parser
---

## 📦 Installation

---

## Quick setup

The following code shows the minimal setup needed for you to get started:

```csharp
  CommandLineParser.WithExceptions = false; //Optionally disable Exceptions (false is default)
  string[] args = ["-n='myName'"];
  Argument<string> nameArg = Argument<string>.Builder()
                .Identifier(new("n", "name"))
                .Build();
  
  CommandLineParser parser = CommandLineParser.Builder()
                .Argument(nameArg)
                .Build();
  
  List<string> errors = await parser.ParseAsync(args);
  //Retrieve value
  Console.WriteLine(nameArg.Value);
```

The example code shows a basic Application using `-n` or `--name` as an argument of type `string`. The result of the 

The following is a subset of what the argument would allow: `-n=MyName`, `-n="My Other Name"`, `-name=Hello`, `-name='Lukbes'`... . See <insert> for more

## Arguments

To create and use your arguments you use the class `Argument<T>`, where `T` is the type of your argument.

An Argument consists of an identifier `ArgumentIdentifier` and a `Description`, a set of rules `IRule<T>` and a set of dependencies to other Arguments `IDependency`. An Argument can be `Required` or optional and have a `DefaultValue`

- `ArgumentIdentifier`: The unique identifier for an Argument consisting of a `shortIdentifier -` and a `longIdentifier --`. `shortIdentifier` is not constrained to one char.
- `IRule<T>`: The rules that will be applied onto this argument. E.g. Must be an HttpLink or Directory. It's checked if and only if the Argument exists and has a value
- `IDependency`: The Dependency constraints that will be checked after the rules where checked. This contains things such as 'Must exist when xyz exists', 'If this exists, force xyz and abc... to exist' etc. . 

### Identifier
```csharp
   var nameArg = Argument<string>.Builder()
                .Identifier(new("n", "name"))
                .Build();
```

### Only Short/Long identifier
```csharp
   var nameArg = Argument<string>.Builder()
                .ShortIdentifier("n")
                .Build();
   
   var longNameArg = Argument<string>.Builder()
                .LongIdentifier("name")
                .Build();
```

### Required

```csharp
    var nameArg = Argument<string>.Builder()
                .Required() 
                .ShortIdentifier("n")
                .Build();
```

### DefaultValue

```csharp
    var nameArg = Argument<string>.Builder()
                .DefaultValue("Tom")
                .ShortIdentifier("n")
                .Build();
```

### Description

```csharp
    var nameArg = Argument<string>.Builder()
                .Description("The name")
                .ShortIdentifier("n")
                .Build();
```

---

### Converter

When defining your Argument type as e.g. string or int, a default converter of type `IConverter<T>` where `T` is the result type is used behind the scenes.

If you don't like the implementation of the default converters, you have some options to change, add or remove them:

#### Custom Converter

You can implement the `IConverter<T>` interface and thus create a new Converter for the type `T`.

The following example shows a point class and a custom converter for this class:

```csharp
    private readonly struct CustomPoint(int X, int Y)
    {
        public override string ToString()
        {
            return "{" + X + ", " + Y + "}"; 
        }
    }
    
    private class CustomPointConverter : IConverter<CustomPoint>
    {
        //Return the errors, or null if successfully
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
```

Now that we have the converter, we can add it to our argument:

```csharp
 CommandLineParser.WithExceptions = false;
 /* var pointArg = Argument<CustomPoint>.Builder()
                .Identifier(new("p", "Point"))
                .Description("The coordinate of the block")
                .Build();
                Would not work, because you have to specify a converter for this type
                Certain types are predefined, see DefaultConverterFactory.Types */
            
 var pointArg = Argument<CustomPoint>.Builder()
     .Identifier(new("p", "Point"))
     .IsRequired()
     .Description("The coordinate of the point")
     .Converter(new CustomPointConverter()) //<- The custom converter
     .Build();
```

#### DefaultConverterFactory

The `DefaultConverterFactory` manages the default converters. The predefined types can be retrieved via 
```csharp
List<Type> types = DefaultConverterFactory.Types;
```

To complete our `CustomPoint` example, we could add the `CustomPointConverter` to the defaults, removing the need to add `.Converter(new CustomPointConverter())` to every Argument of type `CustomPoint`:

```csharp
   bool success = DefaultConverterFactory.TryAdd(new CustomPointConverter());
   foreach (var type in DefaultConverterFactory.Types) //Demonstrating that the type is actually registered (and the others)
   {
       Console.WriteLine(type.Name);
   }
   
   var pointArg = Argument<CustomPoint>.Builder()
       .Identifier(new("p", "Point"))
       .IsRequired()
       .Description("The coordinate of the block")
       .Build(); //No custom converter needed anymore
```

#### Converter for Lists:

The default types also have an implementation of `IConverter<List<T>>`, which means you can use e.g. `Argument<List<int>>` or `Argument<List<string>>` etc. without specifiying a converter.

To use a List of our `CustomPoint`, we have to ensure, that: 

1. We Specified a Converter via `.Converter(new CustomPointConverter())`or added it via `DefaultConverterFactory.TryAdd(new CustomPointConverter());`
2. We Add a list converter via `.ListTypeConverter<CustomPoint>()` which enables it to search for our `CustomPointConverter` that we added in step 1. Or we add a new default converter via `DefaultConverterFactory.TryAddList<CustomPoint>();`  

Step 1 can be skipped if we directly add a new ListConverter<CustomPoint> via `.ListTypeConverter<CustomPoint>(new CustomPointConverter())` or via `DefaultConverterFactory.TryAddList<CustomPoint>(new CustomPointConverter())`.
However this approach does not enable us to use `Argument<CustomPoint>.Builder()...` implicitly, without specifying it explicitly like in the example.

Final example: 
```csharp
DefaultConverterFactory.TryAddList<CustomPoint>(new CustomPointConverter()); //Creating a default only for List<CustomPoint>

var points = Argument<List<CustomPoint>>.Builder()
    .Identifier(new("p", "Points"))
    .IsRequired()
    .Description("The coordinate of the block")
    .Build();

var otherParser = CommandLineParser.Builder().Argument(points).Build();
string[] args = ["-p=10;30,20;40,30;-5"];
var otherErrors = await otherParser.ParseAsync(args);
if (otherErrors.Count == 0)
{
    Console.WriteLine("Worked! ");
    foreach (var point in points.Value)
    {
        Console.WriteLine(point.ToString());
    }
}
```

Result: 
```
Worked! 
{10, 30}
{20, 40}
{30, -5}
```

### Rules

To further constrain your arguments to some rule, you can implement the `IRule<T>` interface. 

Lets say we want our points argument to take at minimum 1 point and at maximum 3 points. For this, we first create a size boundary checking rule for any given Type:

```csharp
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
    //Returns null on success, an error message otherwise
    public string? Validate(Argument<List<T>> argument)
    {
        if (argument.Value!.Count < _lowerBound || argument.Value.Count > _upperBound)
        {
            return $"Count of list must be between {_lowerBound} and {_upperBound}";
        }
        return null;
    }
}
```

Now, to have that rule take affect, we can throw it onto the argument:

```csharp
DefaultConverterFactory.TryAddList<CustomPoint>(new CustomPointConverter());

var points = Argument<List<CustomPoint>>.Builder()
    .Identifier(new("p", "Points"))
    .IsRequired()
    .Rule(new ListCountBetweenXAndY<CustomPoint>(1, 3)) //Setting the boundary to inclusive 1-3
    .Description("The coordinate of the block")
    .Build();

var otherParser = CommandLineParser.Builder().Argument(points).Build();
string[] args = ["-p=10;30,20;40,30;-5,109;2"]; //Will fail, because it has 4 values!
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
```

Result: 

```
Rule failed for '-p, --Points'. Tried value: '10;30,20;40,30;-5,109;2'. Rule: Count of list must be between 1 and 3
```

Another example may be to Have a `CustomPoint` be non-negative in our example. There are two ways to solve this:

1. Create a new Rule implementing `IRule<List<CustomPoint>>`
2. Use the `ListRule<CustomPoint>` class

Option 1:

```csharp
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

//Then use it like so: 

var points = Argument<List<CustomPoint>>.Builder()
                .Identifier(new("p", "Points"))
                .IsRequired()
                .Rule(new ListCountBetweenXAndY<CustomPoint>(1, 3))
                .Rule(new NonNegativePoints()) //<- Our new Rule
                .Description("The coordinate of the block")
                .Build();
```

Option 2:

```csharp
var points = Argument<List<CustomPoint>>.Builder()
                .Identifier(new("p", "Points"))
                .IsRequired()
                .Rule(new ListCountBetweenXAndY<CustomPoint>(1, 3))
                .Rule(new ListRule<CustomPoint>(p => p.X > 0 && p.Y > 0)) //<- Our new Rule
                .Description("The coordinate of the block")
                .Build();
```


### Dependencies

Dependencies are useful if you need argument A to depend on argument B or in the other direction in some certain way. They are called even if the arguments arent provided in `args`, other than `IRule<T>` (Only validates if arg is provided)

A Handful of default dependencies are predefined and usable directly on the Argument:

#### A Requires atleast one of B or C or ... 

Mathematically: A => B or C ...

```csharp
var firstNameArg =  Argument<string>.Builder()
    .Description("The first Name")
    .ShortIdentifier("f")
    .Build();

var ageArg = Argument<int>.Builder()
    .ShortIdentifier("a")
    .Build();

var lastNameArg = Argument<string>.Builder()
    .Description("The Last Name")
    .RequiresOneOf(firstNameArg,  ageArg) //IF last name is present and has a value, first name OR age must have a value 
    .ShortIdentifier("l")
    .Build();
```

#### A Requires all of B and C and ...

Mathematically: A => B and C ...
```csharp
var firstNameArg =  Argument<string>.Builder()
    .Description("The first Name")
    .ShortIdentifier("f")
    .Build();

var ageArg = Argument<int>.Builder()
    .ShortIdentifier("a")
    .Build();

var lastNameArg = Argument<string>.Builder()
    .Description("The Last Name")
    .RequiresAll(firstNameArg,  ageArg) //IF last name is present and has a value, first name AND age must have a value 
    .ShortIdentifier("l")
    .Build();
```

#### A is bound to B and C (A if and only if B or C or ...)

`Bound` means: 
- If A is present -> behaves exactly like `RequiresAll`
- Else if A is not present, but B OR C OR ... is present -> A is also now required.

Mathematically this means: A <=> B or C ...

```csharp
var firstNameArg =  Argument<string>.Builder()
    .Description("The first Name")
    .ShortIdentifier("f")
    .Build();

var ageArg = Argument<int>.Builder()
    .ShortIdentifier("a")
    .Build();

var lastNameArg = Argument<string>.Builder()
    .Description("The Last Name")
    .OnlyWith(firstNameArg, ageArg) lastname if and only if firstname or age
    .ShortIdentifier("l")
    .Build();
```

#### Custom dependencies

If you want to, you can create your own dependencies, which may not be already predefined via implementing the `IDependency` and using it on the argument with `.Dependency(new YourDependency())`.

IDependency works with `IArgument`, so you can depend one argument to any other, no matter the type.

Here is an example, creating a `Dependency` to prevent some argument of being present if this is present: 

```csharp
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
```