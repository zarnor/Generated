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
    public List<InitMember> InitMembers { get; } = new List<InitMember>();

    public string Build()
    {
        var hasCollections = InitMembers.Any(im => im.IsCollection);

        var importedNamespaces = GetImportedNamespaces();

        using (var stringWriter = new StringWriter() { NewLine = NewLine })
        using (var writer = new CodeIndentedTextWriter(stringWriter) { NewLine = NewLine })
        {
            // Usings
            writer.Usings(importedNamespaces);

            // Namespace & Class declaration
            using (writer.Namespace(Namespace))
            using (writer.Class(ClassName, IsPublic))
            {
                // Members
                writer.Members(InitMembers, member =>
                {
                    if (member.IsCollection)
                    {
                        if (member.CollectionType.IsDictionary())
                        {
                            writer.Member($"Dictionary<{member.CollectionType.TypeArguments[0].ToDisplayString()}, {member.CollectionType.TypeArguments[1].ToDisplayString()}>", member.ValueMemberName);
                        }
                        else
                        {
                            writer.Member($"List<{member.TypeName}>", member.ValueMemberName);
                        }
                    }
                    else
                    {
                        writer.Member(member.TypeName, member.ValueMemberName);
                    }
                });

                // Constructor
                writer.Constructor(ClassName);
                writer.Init(ClassName);

                // WithMembers()
                writer.ForeachWithSeparator(InitMembers, member =>
                {
                    member.IsCollection.If(
                        () => member.CollectionType.IsDictionary().If(
                            () => writer.WithDictionary(ClassName, member.Name, member.CollectionType.TypeArguments[0].ToDisplayString(), member.CollectionType.TypeArguments[1].ToDisplayString(), member.ValueMemberName),
                            () => writer.WithCollection(ClassName, member.Name, member.TypeName, member.ValueMemberName)),
                        () => writer.With(ClassName, member.Name, member.TypeName, member.ValueMemberName)
                    );
                });

                // Build()
                var constructorMembers = InitMembers.Where(m => m.CtorIndex.HasValue).ToArray();
                var membersNeedingAdding = InitMembers.Where(m => m.CtorIndex == null && m.IsCollection && !m.HasSetter).ToArray();
                var assignableMembers = InitMembers.Where(m => m.CtorIndex == null && (!m.IsCollection || m.HasSetter)).ToArray();

                writer.Build(TargetClassName,
                    constructorParameterInitialization: constructorMembers.Length == 0 ? null :
                    () =>
                    {
                        for (int i = 0; i < constructorMembers.Length; i++)
                        {
                            var member = constructorMembers[i];
                            var isLast = i == constructorMembers.Length - 1;

                        // TODO: Should convert to list/set/dictionary here or expect constructo to always accept IEnumerable?
                        writer.Write($"{member.ValueMemberName}{(member.IsArray ? ".ToArray()" : "")}{(isLast ? "" : ",")}");
                        }
                    },
                    memberInitialization: assignableMembers.Length == 0 ? null :
                    () =>
                    {
                        for (int i = 0; i < assignableMembers.Length; i++)
                        {
                            var member = assignableMembers[i];
                            var isLast = i == assignableMembers.Length - 1;

                            if (member.CollectionType.IsDictionary())
                            {
                                writer.WriteLine($"{member.Name} = {member.ValueMemberName}{(isLast ? "" : ",")}");
                            }
                            else if (member.CollectionType.IsHashSet())
                            {
                                writer.WriteLine($"{member.Name} = {member.ValueMemberName}.ToHashSet(){(isLast ? "" : ",")}");
                            }
                            else if (member.CollectionType.IsObjectModelCollection())
                            {
                                writer.WriteLine($"{member.Name} = new Collection<{member.TypeName}>({member.ValueMemberName}){(isLast ? "" : ",")}");
                            }
                            else
                            {
                                writer.WriteLine($"{member.Name} = {member.ValueMemberName}{(member.IsArray ? ".ToArray()" : "")}{(isLast ? "" : ",")}");
                            }
                        }
                    },
                    postMemberInitialization: membersNeedingAdding.Length == 0 ? null :
                    () =>
                    {
                        foreach (var member in membersNeedingAdding)
                        {
                            writer.WriteLine($"foreach (var element in {member.ValueMemberName})");
                            writer.BeginScope();
                            writer.WriteLine($"ret.{member.Name}.Add(element);");
                            writer.EndScope();
                        }
                    });
            }

            return stringWriter.ToString();
        }
    }

    private string[] GetImportedNamespaces()
    {
        var namespaces = InitMembers
            .Select(im => im.TypeNamespace);

        namespaces = namespaces.Concat(InitMembers
            .Where(im => im.IsCollection && im.CollectionType.IsDictionary())
            .SelectMany(im => im.CollectionType.TypeArguments)
            .Select(ta => ta.ContainingNamespace.ToString()));

        var hasCollections = InitMembers.Any(im => im.IsCollection);
        if (hasCollections)
        {
            namespaces = namespaces.Concat(new[] { "System.Collections.Generic" });
        }

        var hasObjectModelCollections = InitMembers.Any(im => im.CollectionType.IsObjectModelCollection());
        if (hasObjectModelCollections)
        {
            namespaces = namespaces.Concat(new[] { "System.Collections.ObjectModel" });
        }

        var hasArrays = InitMembers.Any(im => im.IsArray || (im.CollectionType?.IsHashSet() ?? false));
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
