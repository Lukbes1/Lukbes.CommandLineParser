using FluentAssertions;
using Lukbes.CommandLineParser.Arguments;
using Lukbes.CommandLineParser.Arguments.Dependencies;
using Lukbes.CommandLineParser.Arguments.Rules;
using Lukbes.CommandLineParser.Arguments.TypeConverter;
using Moq;

namespace Lukbes.CommandLineParser.Test;

public class ArgumentTest
{
    private readonly Argument<string>.ArgumentBuilder<string> _baseArgumentBuilder =  Argument<string>.Builder().LongIdentifier("test");
    
    [Fact]
    void ApplyValue_Good()
    {
        //arrange, act
        var arg = _baseArgumentBuilder.Build();
        arg.Apply("TestTest");
        
        //asssert
        arg.Value.Should().Be("TestTest");
        arg.HasValue.Should().BeTrue();
    }
    
    [Fact]
    void ApplyValue_Required_Good()
    {
        //arrange
        var arg = _baseArgumentBuilder.IsRequired().Build();
        
        //act
        arg.Apply("TestTest");
        
        //asssert
        arg.Value.Should().Be("TestTest");
        arg.HasValue.Should().BeTrue();
    }
    
    [Fact]
    void ApplyValue_EmptyValue_HasNoDefaultValue()
    {
        //arrange
        CommandLineParser.WithExceptions = false;
        var arg = _baseArgumentBuilder.Build();
        
        //act
        var errors = arg.Apply(null);
        
        //asssert
        errors.Should().BeEmpty();
        arg.HasValue.Should().BeFalse();
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    void ApplyValue_NotProvidedButRequired_ShouldThrow_CommandLineArgumentRequiredException(string? value)
    {
        //arrange
        CommandLineParser.WithExceptions = true;
        var arg = _baseArgumentBuilder.IsRequired().Build();
        
        //act
        var act = () => arg.Apply(value);
        
        //asssert
        act.Should().Throw<CommandLineArgumentRequiredException<string>>("Should throw because value is null or empty and arg was required");
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    void ApplyValue_NotProvidedButRequired_ShouldReturnErrors(string? value)
    {
        //arrange
        CommandLineParser.WithExceptions = false;
        var arg = _baseArgumentBuilder.IsRequired().Build();
        
        //act
        var erros = arg.Apply(value);
        
        //asssert
        erros.Should().NotBeEmpty("Should have errors because value is null or empty and arg was required");
    }
    
    [Fact]
    void ApplyValue_ConverterError_ShouldReturnErrors()
    {
        //arrange
        CommandLineParser.WithExceptions = false;
        var mockRule = new Mock<IConverter<string>>();
        string? testValue = null;
        mockRule
            .Setup(r => r.TryConvert(It.IsAny<string>(), out testValue))
            .Returns("Test Rule error");

        var arg = _baseArgumentBuilder
            .Converter(mockRule.Object)
            .Build();
        
        //act, assert
        var errors = arg.Apply("test");
        errors.Should().HaveCount(1);
        arg.HasValue.Should().BeFalse();
    }
    
    [Fact]
    void ApplyValue_ConverterError_ShouldThrow_CommandLineArgumentConvertException()
    {
        //arrange
        CommandLineParser.WithExceptions = true;
        var mockRule = new Mock<IConverter<string>>();
        string? testValue = null;
        mockRule
            .Setup(r => r.TryConvert(It.IsAny<string>(), out testValue))
            .Returns("Test Rule error");

        var arg = _baseArgumentBuilder
            .Converter(mockRule.Object)
            .Build();
        
        //act, assert
        var act = () => arg.Apply("test");
        act.Should().Throw<CommandLineArgumentConvertException<string>>("Because the converter failed");
    }
    
    [Fact]
    void ApplyValue_ConverterError_ButDefaultValue_ShouldBeGood()
    {
        //arrange
        CommandLineParser.WithExceptions = false;
        var mockRule = new Mock<IConverter<string>>();
        string? testValue = null;
        mockRule
            .Setup(r => r.TryConvert(It.IsAny<string>(), out testValue))
            .Returns("Test Rule error");

        var arg = _baseArgumentBuilder
            .Converter(mockRule.Object)
            .DefaultValue("123")
            .Build();
        
        //act, assert
        var errors = arg.Apply("test");
        errors.Should().HaveCount(1);
        arg.Value.Should().Be("123");
    }

    [Fact]
    void ApplyValue_RuleError_ShouldThrow_CommandLineArgumentRuleException()
    {
        //arrange
        CommandLineParser.WithExceptions = true;
        var mockRule = new Mock<IRule<string>>();
        
        mockRule
            .Setup(r => r.Validate(It.IsAny<Argument<string>>()))
            .Returns("Test Rule error");

        var arg = _baseArgumentBuilder
            .Rule(mockRule.Object)
            .Build();
        
        //act
        var act = () => arg.Apply("TestTest");
        
        //assert
        act.Should().Throw<CommandLineArgumentRuleException>();
    }
    
    [Fact]
    void ApplyValue_RuleError_ShouldReturnErrors()
    {
        //arrange
        CommandLineParser.WithExceptions = false;
        var mockRule = new Mock<IRule<string>>();
        
        mockRule
            .Setup(r => r.Validate(It.IsAny<Argument<string>>()))
            .Returns("Test Rule error");

        var arg = _baseArgumentBuilder
            .Rule(mockRule.Object)
            .Build();
        
        //act, assert
        var errors = arg.Apply("TestTest");
        errors.Should().NotBeEmpty();
    }

    [Fact]
    void ValidateDependencies_EmptyList_NoErrors()
    {
        //arrange
        var arg = _baseArgumentBuilder.Build();
        
        //act
        var errors = arg.ValidateDependencies([]);
        
        //assert
        errors.Should().BeEmpty();
    }
    
    [Fact]
    void ValidateDependencies_ValidDependency_NoErrors()
    {
        //arrange
        var mockDependency = new Mock<IDependency>();
        mockDependency
            .Setup(r => r.Check(It.IsAny<IArgument>(), It.IsAny<HashSet<IArgument>>()))
            .Returns([]);
        var arg = _baseArgumentBuilder.Dependency(mockDependency.Object).Build();
        
        //act
        var errors = arg.ValidateDependencies([Argument<string>.Builder().ShortIdentifier("t").Build()]);
        
        //assert
        errors.Should().BeEmpty();
    }
    
    [Fact]
    void ValidateDependencies_DependencyFailed_ShouldThrow_CommandLineArgumentDependencyException()
    {
        //arrange
        CommandLineParser.WithExceptions = true;
        var mockDependency = new Mock<IDependency>();
        mockDependency
            .Setup(r => r.Check(It.IsAny<IArgument>(), It.IsAny<HashSet<IArgument>>()))
            .Returns(["Has Errors"]);
        var arg = _baseArgumentBuilder.Dependency(mockDependency.Object).Build();
        
        //act
        var act = () => arg.ValidateDependencies([Argument<string>.Builder().ShortIdentifier("t").Build()]);
        
        //assert
        act.Should().Throw<CommandLineArgumentDependencyException>("Because a dependency failed and WithExceptions=true, thus errors should be thrown");
    }
    
    [Fact]
    void ValidateDependencies_DependencyFailed_ShouldReturnErrors()
    {
        //arrange
        CommandLineParser.WithExceptions = false;
        var mockDependency = new Mock<IDependency>();
        mockDependency
            .Setup(r => r.Check(It.IsAny<IArgument>(), It.IsAny<HashSet<IArgument>>()))
            .Returns(["Has Errors"]);
        var arg = _baseArgumentBuilder.Dependency(mockDependency.Object).Build();
        
        //act
        var errors = arg.ValidateDependencies([Argument<string>.Builder().ShortIdentifier("t").Build()]);
        
        //assert
        errors.Should().NotBeEmpty("Because a dependency failed, thus errors should be returned");
    }

    [Fact]
    void ToString_good()
    {
        var arg = _baseArgumentBuilder.Build();
        arg.ToString().Should().Contain($"[--{arg.Identifier.LongIdentifier}]");
    }
    
    [Fact]
    void ToString_short()
    {
        var arg = _baseArgumentBuilder.ShortIdentifier("t").Build();
        arg.ToString().Should().Contain($"-t");
    }

    [Fact]
    void ToString_Default()
    {
        var arg = _baseArgumentBuilder.DefaultValue("test").Build();
        arg.ToString().Should().Contain($"(Default: {arg.DefaultValue})");
    }
    
    [Fact]
    void ToString_required()
    {
        var arg = _baseArgumentBuilder.IsRequired().Build();
        arg.ToString().Should().Contain($"--{arg.Identifier.LongIdentifier}");
    }
    
    #region builder

       private class CommandLineParserMockConverter : IConverter<CommandLineParser>
    {
        public string? TryConvert(string? value, out CommandLineParser? result)
        {
            result = null;
            return null;
        }
    }

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