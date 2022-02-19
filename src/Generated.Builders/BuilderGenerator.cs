using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Generated.Builders;

internal class BuilderGenerator
{
    public string Namespace { get; internal set; }
    public string ClassName { get; internal set; }
    public string TargetClassName { get; internal set; }
    public bool IsPublic { get; internal set; }
    public string NewLine { get; internal set; }
    public IList<InitMember> InitMembers { get; } = new List<InitMember>();

    public string Build()
    {
        var importedNamespaces = InitMembers
            .Select(im => im.TypeNamespace)
            .Where(ns => ns != Namespace)
            .Distinct()
            .OrderBy(ns => ns.StartsWith("System.") ? 0 : 1)
            .ThenBy(ns => ns)
            .ToArray();

        using (var writer = new StringWriter() { NewLine = NewLine })
        using (var indentWriter = new IndentedTextWriter(writer) { NewLine = NewLine })
        {
            // Usings
            if (importedNamespaces.Length > 0)
            {
                foreach (var ns in importedNamespaces)
                {
                    indentWriter.WriteLine($"using {ns};");
                }

                indentWriter.WriteEmptyLine();
            }

            // Namespace
            indentWriter.WriteLine($"namespace {Namespace}");
            indentWriter.BeginScope();

            // Constructor
            indentWriter.WriteLine($"{(IsPublic ? "public" : "internal")} partial class {ClassName}");
            indentWriter.BeginScope();

            if (InitMembers.Count > 0)
            {
                foreach (var member in InitMembers)
                {
                    indentWriter.WriteLine($"private {member.TypeName} {member.ValueMemberName};");
                }

                indentWriter.WriteEmptyLine();
            }

            indentWriter.WriteLine($"private {ClassName}()");
            indentWriter.BeginScope();
            indentWriter.EndScope(); // end constructor

            // Init()
            indentWriter.WriteEmptyLine();
            indentWriter.WriteLine($"public static {ClassName} Init()");
            indentWriter.BeginScope();
            indentWriter.WriteLine($"return new {ClassName}();");
            indentWriter.EndScope(); // end Init()

            // WithMembers()
            foreach (var member in InitMembers)
            {
                indentWriter.WriteEmptyLine();
                indentWriter.WriteLine($"public {ClassName} With{member.Name}({member.TypeName} value)");
                indentWriter.BeginScope();
                indentWriter.WriteLine($"{member.ValueMemberName} = value;");
                indentWriter.WriteLine($"return this;");
                indentWriter.EndScope();
            }

            // Build()
            indentWriter.WriteEmptyLine();
            indentWriter.WriteLine($"public {TargetClassName} Build()");
            indentWriter.BeginScope();

            if (InitMembers.Count > 0)
            {
                indentWriter.WriteLine($"return new {TargetClassName}()");
                indentWriter.BeginScope();
                for (int i = 0; i < InitMembers.Count; i++)
                {
                    var member = InitMembers[i];
                    var isLast = i == InitMembers.Count - 1;
                    indentWriter.WriteLine($"{member.Name} = {member.ValueMemberName}{(isLast ? "" : ",")}");
                }
                indentWriter.EndScope(";");
            }
            else
            {
                indentWriter.WriteLine($"return new {TargetClassName}();");
            }

            indentWriter.EndScope(); // end Build()

            indentWriter.EndScope(); // end class
            indentWriter.EndScope(); // end namespace

            return writer.ToString();
        }
    }
}
