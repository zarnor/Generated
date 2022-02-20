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
        var hasCollections = InitMembers.Any(im => im.IsCollection);

        var importedNamespaces = GetImportedNamespaces();

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

            // Class declaration
            indentWriter.WriteLine($"{(IsPublic ? "public" : "internal")} partial class {ClassName}");
            indentWriter.BeginScope();

            // Members
            if (InitMembers.Count > 0)
            {
                foreach (var member in InitMembers)
                {
                    if (member.IsCollection)
                    {
                        indentWriter.WriteLine($"private List<{member.TypeName}> {member.ValueMemberName};");
                    }
                    else
                    {
                        indentWriter.WriteLine($"private {member.TypeName} {member.ValueMemberName};");
                    }
                }

                indentWriter.WriteEmptyLine();
            }

            // Constructor
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
                if (member.IsCollection)
                {
                    indentWriter.WriteEmptyLine();
                    indentWriter.WriteLine($"public {ClassName} With{member.Name}(IEnumerable<{member.TypeName}> values)");
                    indentWriter.BeginScope();
                    indentWriter.WriteLine($"{member.ValueMemberName} = new List<{member.TypeName}>(values);");
                    indentWriter.WriteLine($"return this;");
                    indentWriter.EndScope();

                    indentWriter.WriteEmptyLine();
                    indentWriter.WriteLine($"public {ClassName} Add{member.Name.ToSingular()}({member.TypeName} value)");
                    indentWriter.BeginScope();
                    indentWriter.WriteLine($"if ({member.ValueMemberName} == null)");
                    indentWriter.BeginScope();
                    indentWriter.WriteLine($"{member.ValueMemberName} = new List<{member.TypeName}>();");
                    indentWriter.EndScope();
                    indentWriter.WriteEmptyLine();
                    indentWriter.WriteLine($"{member.ValueMemberName}.Add(value);");
                    indentWriter.WriteEmptyLine();
                    indentWriter.WriteLine($"return this;");
                    indentWriter.EndScope();
                }
                else
                {
                    indentWriter.WriteEmptyLine();
                    indentWriter.WriteLine($"public {ClassName} With{member.Name}({member.TypeName} value)");
                    indentWriter.BeginScope();
                    indentWriter.WriteLine($"{member.ValueMemberName} = value;");
                    indentWriter.WriteLine($"return this;");
                    indentWriter.EndScope();
                }
            }

            // Build()
            indentWriter.WriteEmptyLine();
            indentWriter.WriteLine($"public {TargetClassName} Build()");
            indentWriter.BeginScope();

            var membersNeedingAdding = InitMembers.Where(m => m.IsCollection && !m.HasSetter).ToArray();
            var assignableMembers = InitMembers.Where(m => !m.IsCollection || m.HasSetter).ToArray();

            if (InitMembers.Count > 0)
            {
                bool needsVariable = membersNeedingAdding.Length > 0;
                indentWriter.WriteLine($"{(needsVariable ? "var ret =" : "return")} new {TargetClassName}(){(assignableMembers.Length > 0 ? "" : ";" )}");

                // Members that can be assigned with init accessors
                if (assignableMembers.Length > 0)
                {
                    indentWriter.BeginScope();
                    for (int i = 0; i < assignableMembers.Length; i++)
                    {
                        var member = assignableMembers[i];
                        var isLast = i == assignableMembers.Length - 1;

                        indentWriter.WriteLine($"{member.Name} = {member.ValueMemberName}{(member.IsArray ? ".ToArray()" : "")}{(isLast ? "" : ",")}");
                    }
                    indentWriter.EndScope(";");
                }

                // Members that 
                if (membersNeedingAdding.Length > 0)
                {
                    for (int i = 0; i < membersNeedingAdding.Length; i++)
                    {
                        var member = membersNeedingAdding[i];
                        indentWriter.WriteLine($"foreach (var element in {member.ValueMemberName})");
                        indentWriter.BeginScope();
                        indentWriter.WriteLine($"ret.{member.Name}.Add(element);");
                        indentWriter.EndScope();
                    }

                    indentWriter.WriteLine($"return ret;");
                }
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

    private string[] GetImportedNamespaces()
    {
        var namespaces = InitMembers
            .Select(im => im.TypeNamespace);

        var hasCollections = InitMembers.Any(im => im.IsCollection);
        if (hasCollections)
        {
            namespaces = namespaces.Concat(new[] { "System.Collections.Generic" });
        }

        var hasArrays = InitMembers.Any(im => im.IsArray);
        if (hasArrays)
        {
            namespaces = namespaces.Concat(new[] { "System.Linq" });
        }

        return namespaces
            .Where(ns => ns != Namespace)
            .Distinct()
            .OrderBy(ns => ns.StartsWith("System") ? 0 : 1)
            .ThenBy(ns => ns)
            .ToArray();
    }
}
