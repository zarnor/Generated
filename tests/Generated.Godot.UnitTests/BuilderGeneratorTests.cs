using System.Text;
using System.Threading.Tasks;
using Godot;
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
using Godot;

public class Label {}
public class Node2D
{
    public virtual void _EnterTree() {}
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
    public override void _EnterTree()
    {
        base._EnterTree();

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
                    (typeof(GodotSourceGenerator), "GetNodeAttribute.g.cs", GetNodeAttribute.GetSourceCode()),
                    (typeof(GodotSourceGenerator), "MyScene.generated.godot.g.cs", SourceText.From(expected, Encoding.UTF8, SourceHashAlgorithm.Sha256)),
                },
            },
        }.RunAsync();
    }

    [Fact]
    public async Task Calls_EnterTree_Function_When_Defined()
    {
        var code = @"using System;
using Godot;

public class Label {}
public class Node2D
{
    public virtual void _EnterTree() {}
    public T GetNode<T>(string path)
    {
        return default(T);
    }
}

public partial class MyScene : Node2D
{
    [GetNode(""MyLabel"")]
    private Label _myLabel;

    private void EnterTree()
    {
    }
}
";
        var expected = @"public partial class MyScene
{
    public override void _EnterTree()
    {
        base._EnterTree();

        _myLabel = GetNode<Label>(""MyLabel"");

        EnterTree();
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
                    (typeof(GodotSourceGenerator), "GetNodeAttribute.g.cs", GetNodeAttribute.GetSourceCode()),
                    (typeof(GodotSourceGenerator), "MyScene.generated.godot.g.cs", SourceText.From(expected, Encoding.UTF8, SourceHashAlgorithm.Sha256)),
                },
            },
        }.RunAsync();
    }

    [Fact]
    public async Task Reports_When_Conflicting_EnterTree_Function_Defined()
    {
        var code = @"using System;
using Godot;

public class Label {}
public class Node2D
{
    public virtual void _EnterTree() {}
    public T GetNode<T>(string path)
    {
        return default(T);
    }
}

public partial class MyScene : Node2D
{
    [GetNode(""MyLabel"")]
    private Label _myLabel;

    private void {|GG1:_EnterTree|}()
    {
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
                    (typeof(GodotSourceGenerator), "GetNodeAttribute.g.cs", GetNodeAttribute.GetSourceCode()),
                }
            },
        }.RunAsync();
    }
}
