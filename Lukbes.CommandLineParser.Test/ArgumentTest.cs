using FluentAssertions;
using Lukbes.CommandLineParser.Arguments;
using Lukbes.CommandLineParser.Arguments.TypeConverter;

namespace Lukbes.CommandLineParser.Test;

public class ArgumentTest
{
    
    
    #region builder

       private class CommandLineParserMockConverter : IConverter<CommandLineParser>
    {
        public string? TryConvert(string? value, out CommandLineParser? result)
        {
            result = null;
            return null;
        }
    }
    
    private readonly Argument<string>.ArgumentBuilder<string> _baseArgumentBuilder =  new Argument<string>.ArgumentBuilder<string>().LongIdentifier("test");

    [Fact]
    void When_Builder_Identifer_HasIdentifier()
    {
        //arrange
        ArgumentIdentifier expected = new ArgumentIdentifier("id");
        
        //act
        var argument = Argument<string>.Builder().Identifier(expected).Build();
        
        //assert
        argument.Identifier.Should().BeEquivalentTo(expected);
    }
    
    [Fact]
    void When_Builder_ShortIdentifier_HasShortIdentifier()
    {
        //arrange
        string shortIdentifier = "id";
        
        //act
        var argument = Argument<string>.Builder().ShortIdentifier(shortIdentifier).Build();
        
        //assert
        argument.Identifier.ShortIdentifier.Should().BeEquivalentTo(shortIdentifier);
    }
    
    [Fact]
    void When_Builder_LongIdentifier_HasLongIdentifier()
    {
        //arrange
        string longIdentifier = "id";
        
        //act
        var argument = Argument<string>.Builder().LongIdentifier(longIdentifier).Build();
        
        //assert
        argument.Identifier.LongIdentifier.Should().BeEquivalentTo(longIdentifier);
    }

    [Fact]
    void When_Builder_NoIdentifier_Throws()
    {
        //arrange
        //act
        //assert
        var argument = Argument<string>.Builder();
        var act = () => argument.Build();
        act.Should().Throw<CommandLineArgumentIdentifierException>();
    }
    
    [Fact]
    void When_Builder_NoDefaultConverter_Throws()
    {
        //arrange
        //act
        //assert
        var argWithRandomType = Argument<CommandLineParser>.Builder().LongIdentifier("Stupid Arg");
        var act = () => argWithRandomType.Build();
        act.Should().Throw<BuilderPropertyNullOrEmptyException<IConverter<CommandLineParser>>>("Should throw because this type has no default converter");
    }
    
    [Fact]
    void When_Builder_NoDefaultConverter_ButCustomConverter_DoesNotThrow()
    {
        //arrange
        //act
        //assert
        var argWithRandomType = Argument<CommandLineParser>.Builder().Converter(new CommandLineParserMockConverter()).LongIdentifier("Stupid Arg");
        var act = () => argWithRandomType.Build();
        act.Should().NotThrow("Should not throw because a custom converter is provided");
    }
    
    [Fact]
    void When_Builder_NoDefinedConverter_FindsNoConverter()
    {
        //arrange
        //act
        //assert
        var act = () => _baseArgumentBuilder.Build();
        act.Should().NotThrow("The converter should be found because string has a default converter defined");
    }

    [Fact]
    void When_Builder_Description_HasDescription()
    {
        //arrange
        string description = "id";
        
        //act
        var argument = _baseArgumentBuilder.Description(description).Build();
        
        //assert
        argument.Description.Should().BeEquivalentTo(description);
    }
    
    [Fact]
    void When_Builder_IsRequired_HasIsRequired()
    {
        //arrange
        //act
        var argument = _baseArgumentBuilder.IsRequired().Build();
        
        //assert
        argument.IsRequired.Should().BeTrue();
    }
    
    [Fact]
    void When_Builder_DefaultValue_HasDefaults_HasValue()
    {
        //arrange
        string defaultValue = "default";
        
        //act
        var argument = _baseArgumentBuilder.DefaultValue(defaultValue).Build();
        
        //assert
        argument.HasDefaultValue.Should().BeTrue();
        argument.HasValue.Should().BeTrue();
        argument.Value.Should().BeEquivalentTo(defaultValue);
        argument.DefaultValue.Should().BeEquivalentTo(defaultValue);
    }

    #endregion
}