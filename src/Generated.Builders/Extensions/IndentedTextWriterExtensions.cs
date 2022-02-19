using System.CodeDom.Compiler;

namespace Generated.Builders;

internal static class IndentedTextWriterExtensions
{
    public static void BeginScope(this IndentedTextWriter writer)
    {
        writer.WriteLine("{");
        writer.Indent++;
    }

    public static void EndScope(this IndentedTextWriter writer, string text = null)
    {
        writer.Indent--;
        writer.WriteLine("}" + text);
    }

    public static void WriteEmptyLine(this IndentedTextWriter writer)
    {
        writer.WriteLineNoTabs(string.Empty);
    }
}
