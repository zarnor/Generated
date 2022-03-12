using System;
using System.IO;
using System.Linq;
using System.Text;
using Generated.Godot.Extensions;
using Godot;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Generated.Godot;

[Generator]
internal class GodotSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // Registers the attribute source to be part of the compilation.
        // It is required since the source generator assembly won't be a dependency in the final assembly.
        // Also without this the attribute type details won't be available later.
        context.RegisterForPostInitialization((i) => i.AddSource("GetNodeAttribute.g.cs", GetNodeAttribute.GetSourceCode()));

        // The generator infrastructure will call the syntax receiver to populate it.
        context.RegisterForSyntaxNotifications(() => new GodotSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var syntaxReceiver = (GodotSyntaxReceiver)context.SyntaxContextReceiver!;

        // Find the attribute symbol to compare with.
        // Also seems that attribute constructor parameters are not resolved without this.
        var attributeSymbol = context.Compilation.GetTypeByMetadataName(GetNodeAttribute.FullName)!;

        foreach (var classToAugment in syntaxReceiver.ClassesToAugment)
        {
            var model = context.Compilation.GetSemanticModel(classToAugment.SyntaxTree);
            var classSymbol = model.GetDeclaredSymbol(classToAugment, context.CancellationToken) ?? throw new Exception("Could not get declared symbol for class.");
            string? classNamespace = classSymbol.ContainingNamespace.IsGlobalNamespace ? null : classSymbol.ContainingNamespace.ToString();

            using (var stringWriter = new StringWriter())
            using (var writer = new CodeIndentedTextWriter(stringWriter) { NewLine = Environment.NewLine })
            {
                using (writer.Namespace(classNamespace))
                using (writer.Class(classSymbol.Name, classSymbol.DeclaredAccessibility == Accessibility.Public))
                {
                    writer.WriteLine("public override void _EnterTree()");
                    writer.BeginScope();
                    writer.WriteLine("base._EnterTree();");
                    writer.WriteSeparatorLine();

                    var membersWithGetNodeAttributes = classSymbol.GetMembers()
                        .OfType<IFieldSymbol>()
                        .Select(field => new {
                            Name = field.Name,
                            Type = field.Type.ToDisplayString(),
                            Path = field.GetAttribute(attributeSymbol)?.GetFirstConstructorArgument()?.Value as string
                        })
                        .Where(m => m.Path != null);

                    foreach (var member in membersWithGetNodeAttributes)
                    {
                        writer.WriteLine($"{member.Name} = GetNode<{member.Type}>(\"{member.Path}\");");
                    }

                    var conflictingEnterTreeFunction = classSymbol.GetMembers("_EnterTree").OfType<IMethodSymbol>().FirstOrDefault(m => !m.Parameters.Any());
                    if (conflictingEnterTreeFunction != null)
                    {
                        var node = conflictingEnterTreeFunction.DeclaringSyntaxReferences.First().GetSyntax();
                        var token = node.ChildTokens().First(t => t.IsKind(SyntaxKind.IdentifierToken));
                        context.ReportDiagnostic(RenameReadyFunctionDiagnostic.Create(token.GetLocation()));
                        continue;
                    }

                    bool hasEnterTreeFunction = classSymbol.GetMembers("EnterTree").OfType<IMethodSymbol>().Any(m => !m.Parameters.Any());
                    if (hasEnterTreeFunction)
                    {
                        writer.WriteSeparatorLine();
                        writer.WriteLine("EnterTree();");
                    }

                    writer.EndScope();
                }

                var source = stringWriter.ToString();
                context.AddSource($"{classSymbol.Name}.generated.godot.g.cs", SourceText.From(source, Encoding.UTF8, SourceHashAlgorithm.Sha256));
            }
        }
    }
}
