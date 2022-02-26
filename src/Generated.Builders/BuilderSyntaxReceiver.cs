using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Generated.Builders;

/// <summary>
/// Receives the syntax nodes and determines which classes have been marked to augment.
/// </summary>
internal class BuilderSyntaxReceiver : ISyntaxContextReceiver
{
    public List<TypeDeclarationSyntax> ClassesToAugment { get; } = new List<TypeDeclarationSyntax>();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        var syntaxNode = context.Node;
        var model = context.SemanticModel;

        if (syntaxNode is TypeDeclarationSyntax typeDeclarationSyntax)
        {
            var flexBuilderAttribute = typeDeclarationSyntax.AttributeLists
                .SelectMany(al => al.ChildNodes().OfType<AttributeSyntax>())
                .Where(a => a.Name.ToString() == "FlexibleBuilder" ||
                            a.Name.ToString() == "Generated.Builders.FlexibleBuilder")
                .FirstOrDefault();

            if (flexBuilderAttribute == null)
            {
                return;
            }

            ClassesToAugment.Add(typeDeclarationSyntax);
        }
    }
}
