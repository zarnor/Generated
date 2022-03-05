using System.Linq;
using Microsoft.CodeAnalysis;

namespace Generated.Godot.Extensions;

internal static class SymbolExtensions
{
    public static AttributeData? GetAttribute(this ISymbol symbol, INamedTypeSymbol attributeType)
        => symbol.GetAttributes().FirstOrDefault(a => attributeType.Equals(a.AttributeClass, SymbolEqualityComparer.Default));

    public static TypedConstant? GetFirstConstructorArgument(this AttributeData attribute)
        => attribute.ConstructorArguments.FirstOrDefault();
}
