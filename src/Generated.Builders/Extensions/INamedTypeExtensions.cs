using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Generated.Builders;

internal static class INamedTypeExtensions
{
    public static IEnumerable<ISymbol> GetAllMembers(this INamedTypeSymbol symbol)
    {
        for (var currentSymbol = symbol; currentSymbol != null; currentSymbol = currentSymbol.BaseType)
        {
            foreach (var member in currentSymbol.GetMembers())
            {
                yield return member;
            }
        }
    }
}
