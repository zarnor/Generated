using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Generated.Godot;

/// <summary>
/// Receives the syntax nodes and determines which classes need to be augmented.
/// </summary>
internal class GodotSyntaxReceiver : ISyntaxContextReceiver
{
    public ISet<TypeDeclarationSyntax> ClassesToAugment { get; } = new HashSet<TypeDeclarationSyntax>();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        var syntaxNode = context.Node;
        var model = context.SemanticModel;

        if (syntaxNode is not MemberDeclarationSyntax memberDeclarationSyntax)
        {
            return;
        }

        var classToAugment = memberDeclarationSyntax.AttributeLists
            .SelectMany(al => al.ChildNodes().OfType<AttributeSyntax>())
            .Where(a => a.Name.ToString() == "GetNode" ||
                        a.Name.ToString() == "Generated.Godot.GetNode")
            .FirstOrDefault()
            ?.FirstAncestorOrSelf<TypeDeclarationSyntax>();

        if (classToAugment != null)
        {
            ClassesToAugment.Add(classToAugment);
        }
    }
}
