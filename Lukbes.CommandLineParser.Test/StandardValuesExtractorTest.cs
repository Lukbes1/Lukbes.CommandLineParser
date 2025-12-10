using FluentAssertions;
using Lukbes.CommandLineParser.Arguments;
using Lukbes.CommandLineParser.Extracting;

namespace Lukbes.CommandLineParser.Test;

public class StandardValuesExtractorTest
{
    private readonly StandardValuesExtractor _extractor = new();

    public static IEnumerable<object[]> GoodArgumentValues =>
        new List<object[]>
        {
            new object[]
            {
                new[] { "-h" },
                new[] { new ArgumentIdentifier(shortIdentifier: "h") },
                new[] { "true" }
            },
            new object[]
            {
                new[] { "--output=file.txt" },
                new[] { new ArgumentIdentifier(longIdentifier: "output") },
                new[] { "file.txt" }
            },
            new object[]
            {
                new[] { "-abc123" },
                new[] { new ArgumentIdentifier(shortIdentifier: "abc123") },
                new[] { "true" }
            },
            new object[]
            {
                new[] { "--apastrophe='living la vida loca'" },
                new[] { new ArgumentIdentifier(longIdentifier: "apastrophe") },
                new[] { "living la vida loca" }
            },
            new object[]
            {
                new[] { "--quotationMarks=\"living la vida loca\"" },
                new[] { new ArgumentIdentifier(longIdentifier: "quotationMarks") },
                new[] { "living la vida loca" }
            },
            new object[]
            {
                new[] { "-f=\"I love csharp\"", "--second='java is fine too'", "--r=10.3" },
                new[]
                {
                    new ArgumentIdentifier(shortIdentifier: "f"), new ArgumentIdentifier(longIdentifier: "second"),
                    new ArgumentIdentifier(longIdentifier: "r")
                },
                new[] { "I love csharp", "java is fine too", "10.3" }
            }
        };
    
    [Theory]
    [MemberData(nameof(GoodArgumentValues))]
    void ExtractGood_ShouldHave_Identifiers_And_Values(string[] args, ArgumentIdentifier[] identifiers, string[] values)
    {
        var result = _extractor.Extract(args);
        result.errors.Should().BeEmpty();
        
        result.identifierAndValues.Keys.Should().Contain(identifiers);
        result.identifierAndValues.Values.Should().Contain(values);
    }
    

    [Theory]
    [InlineData("---h")]
    [InlineData("h")]
    [InlineData("salsa")]
    [InlineData("--h=")]
    [InlineData("--h='''")]
    [InlineData("--h='bad' a")]
    [InlineData("a --h='bad'")]
    [InlineData("-a-=\"")]
    void ExtractBad_ShouldHaveErrors(string arg)
    {
        var result = _extractor.Extract([arg]);
        result.errors.Should().NotBeEmpty();
    }
    
}