using System;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace Generated.Godot;

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

    public static SourceText GetSourceCode()
    {
        return SourceText.From(@"using System;

namespace Generated.Godot
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
