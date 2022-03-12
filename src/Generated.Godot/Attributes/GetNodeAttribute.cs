using System;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace Godot;

/// <summary>
/// Get node instance in the OnReady() handler.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class GetNodeAttribute : Attribute
{
    public GetNodeAttribute(string path)
    {
        Path = path ?? throw new ArgumentNullException(nameof(path));
    }

    public string Path { get; set; }

    internal static string FullName { get; } = "Godot.GetNodeAttribute";

    internal static string[] SyntaxNames { get; } = new string[]
    {
        "GetNode",
        "GetNodeAttribute",
        "Godot.GetNode",
        "Godot.GetNodeAttribute"
    };

    public static SourceText GetSourceCode()
    {
        return SourceText.From(@"using System;

namespace Godot
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    internal class GetNodeAttribute : Attribute
    {
        public GetNodeAttribute(string path)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public string Path { get; set; }
    }
}
", Encoding.UTF8, SourceHashAlgorithm.Sha256);
    }
}
