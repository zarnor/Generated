using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using Xunit;

// https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md#unit-testing-of-generators
using VerifyCS = Generated.Builders.UnitTests.CSharpSourceGeneratorVerifier<Generated.Builders.BuilderSourceGenerator>;

namespace Generated.Builders.UnitTests;

public class CollectionTests
{
    [InlineData("System.Collections.Generic", "HashSet", "System.Linq", "_list.ToHashSet()")]
    [InlineData("System.Collections.Generic", "ISet", "System.Linq", "_list.ToHashSet()")]
    [InlineData("System.Collections.Generic", "List", null, "_list")]
    [InlineData("System.Collections.Generic", "IList", null, "_list")]
    [InlineData("System.Collections.ObjectModel", "Collection", "System.Collections.ObjectModel", "new Collection<string>(_list)")]
    [InlineData("System.Collections.Generic", "ICollection", null, "_list")]
    [Theory]
    public Task GenericCollections(string import, string listType, string? expectedImport, string initializerCode)
        => VerifyList(import, listType, expectedImport, initializerCode);

    [InlineData("System.Collections.Generic", "Dictionary<int, string>", null, "_list")]
    [InlineData("System.Collections.Generic", "IDictionary<int, string>", null, "_list")]
    [Theory]
    public Task GenericDictionaries(string import, string listType, string? expectedImport, string initializerCode)
        => VerifyDictionary(import, listType, expectedImport, initializerCode);

    private async Task VerifyList(string import, string listType, string? expectedImport, string initializerCode)
    {
        var codeUsings = string.Join("\n", (new[] { "Generated.Builders", import }).Where(x => x != null).OrderBy(x => x).Select(x => $"using {x};"));
        var expectedUsings = string.Join("\n", (new[] { "System", "System.Collections.Generic", expectedImport }).Where(x => x != null).OrderBy(x => x).Select(x => $"using {x};"));

        var code = @"using System;
" + codeUsings + @"

public class Dto
{
    public " + listType + @"<string> List { get; set; }
}

[FlexibleBuilder(typeof(Dto))]
public partial class DtoBuilder
{
}
";
        var expected = expectedUsings + @"

public partial class DtoBuilder
{
    private List<string> _list;

    private DtoBuilder()
    {
    }

    public static DtoBuilder Init()
    {
        return new DtoBuilder();
    }

    public DtoBuilder WithList(IEnumerable<string> values)
    {
        _list = new List<string>(values);
        return this;
    }

    public DtoBuilder AddToList(string value)
    {
        if (_list == null)
        {
            _list = new List<string>();
        }

        _list.Add(value);

        return this;
    }

    public Dto Build()
    {
        return new Dto()
        {
            List = " + initializerCode + @"
        };
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

    private async Task VerifyDictionary(string import, string listType, string? expectedImport, string initializerCode)
    {
        var codeUsings = string.Join("\n", (new[] { "Generated.Builders", import }).Where(x => x != null).OrderBy(x => x).Select(x => $"using {x};"));
        var expectedUsings = string.Join("\n", (new[] { "System", "System.Collections.Generic", expectedImport }).Where(x => x != null).OrderBy(x => x).Select(x => $"using {x};"));

        var code = @"using System;
" + codeUsings + @"

public class Dto
{
    public " + listType + @" List { get; set; }
}

[FlexibleBuilder(typeof(Dto))]
public partial class DtoBuilder
{
}
";
        var expected = expectedUsings + @"

public partial class DtoBuilder
{
    private Dictionary<int, string> _list;

    private DtoBuilder()
    {
    }

    public static DtoBuilder Init()
    {
        return new DtoBuilder();
    }

    public DtoBuilder WithList(IDictionary<int, string> values)
    {
        _list = new Dictionary<int, string>(values);
        return this;
    }

    public DtoBuilder AddToList(int key, string value)
    {
        if (_list == null)
        {
            _list = new Dictionary<int, string>();
        }

        _list.Add(key, value);

        return this;
    }

    public Dto Build()
    {
        return new Dto()
        {
            List = " + initializerCode + @"
        };
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
