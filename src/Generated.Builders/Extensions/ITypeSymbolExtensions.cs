using Microsoft.CodeAnalysis;

namespace Generated.Builders;

internal static class ITypeSymbolExtensions
{
    public static string ToSimplifiedString(this ITypeSymbol symbol)
        => symbol.Name switch
        {
            "String" => "string",
            _ => symbol.Name
        };
}
