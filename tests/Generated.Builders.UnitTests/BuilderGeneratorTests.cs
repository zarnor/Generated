using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using Xunit;

// https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md#unit-testing-of-generators
using VerifyCS = Generated.Builders.UnitTests.CSharpSourceGeneratorVerifier<Generated.Builders.BuilderSourceGenerator>;

namespace Generated.Builders.UnitTests;

public class BuilderGeneratorTests
{
    [Fact]
    public async Task Builds_Simple_Dto()
    {
        var code = @"using System;
using Generated.Builders;

namespace TestProject.Models;

public class Customer
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

[FlexibleBuilder(typeof(Customer))]
public partial class CustomerBuilder
{
}
";
        var expected = @"using System;

namespace TestProject.Models
{
    public partial class CustomerBuilder
    {
        private string _firstName;
        private string _lastName;

        private CustomerBuilder()
        {
        }

        public static CustomerBuilder Init()
        {
            return new CustomerBuilder();
        }

        public CustomerBuilder WithFirstName(string value)
        {
            _firstName = value;
            return this;
        }

        public CustomerBuilder WithLastName(string value)
        {
            _lastName = value;
            return this;
        }

        public Customer Build()
        {
            return new Customer()
            {
                FirstName = _firstName,
                LastName = _lastName
            };
        }
    }
}
";

        await new VerifyCS.Test
        {
            TestState =
            {
                Sources = { code },
                GeneratedSources =
                {
                    (typeof(BuilderSourceGenerator), "FlexibleBuilderAttribute.g.cs", FlexibleBuilderAttribute.GetSourceCode()),
                    (typeof(BuilderSourceGenerator), "Generated.Builders.cs", SourceText.From(expected, Encoding.UTF8, SourceHashAlgorithm.Sha256)),
                },
            },
        }.RunAsync();
    }
}
