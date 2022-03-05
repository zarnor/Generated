using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using Xunit;

// https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md#unit-testing-of-generators
using VerifyCS = Generated.Builders.UnitTests.CSharpSourceGeneratorVerifier<Generated.Godot.GodotSourceGenerator>;

namespace Generated.Godot.UnitTests;

public class BuilderGeneratorTests
{
    [Fact]
    public async Task Creates_Ready_Function()
    {
        var code = @"using System;
using Generated.Godot;

public class Label {}
public class Node2D
{
    public virtual void _Ready() {}
    public T GetNode<T>(string path)
    {
        return default(T);
    }
}

public partial class MyScene : Node2D
{
    [GetNode(""MyLabel"")]
    private Label _myLabel;
}
";
        var expected = @"public partial class MyScene
{
    public override void _Ready()
    {
        _myLabel = GetNode<Label>(""MyLabel"");
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
                    (typeof(GodotSourceGenerator), "GetPathAttribute.g.cs", GetNodeAttribute.GetSourceCode()),
                    (typeof(GodotSourceGenerator), "MyScene.generated.godot.g.cs", SourceText.From(expected, Encoding.UTF8, SourceHashAlgorithm.Sha256)),
                },
            },
        }.RunAsync();
    }

    [Fact]
    public async Task Calls_Ready_Function_When_Defined()
    {
        var code = @"using System;
using Generated.Godot;

public class Label {}
public class Node2D
{
    public virtual void _Ready() {}
    public T GetNode<T>(string path)
    {
        return default(T);
    }
}

public partial class MyScene : Node2D
{
    [GetNode(""MyLabel"")]
    private Label _myLabel;

    private void Ready()
    {
    }
}
";
        var expected = @"public partial class MyScene
{
    public override void _Ready()
    {
        _myLabel = GetNode<Label>(""MyLabel"");

        Ready();
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
                    (typeof(GodotSourceGenerator), "GetPathAttribute.g.cs", GetNodeAttribute.GetSourceCode()),
                    (typeof(GodotSourceGenerator), "MyScene.generated.godot.g.cs", SourceText.From(expected, Encoding.UTF8, SourceHashAlgorithm.Sha256)),
                },
            },
        }.RunAsync();
    }
}
