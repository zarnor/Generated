using System.Linq;
using Microsoft.CodeAnalysis;

namespace Generated.Builders;

internal static class ITypeSymbolExtensions
{
    public static string ToSimplifiedString(this ITypeSymbol symbol)
    {
        if (symbol.ContainingNamespace.Name != "System")
        {
            return symbol.Name;
        }

        return symbol.Name switch
        {
            "Boolean" => "bool",
            "Int16" => "short",
            "Int32" => "int",
            "Int64" => "long",
            "UInt16" => "ushort",
            "UInt32" => "uint",
            "UInt64" => "ulong",
            "Char" => "char",
            "Byte" => "byte",
            "SByte" => "sbyte",
            "Decimal" => "decimal",
            "Double" => "double",
            "Float" => "float",
            "String" => "string",
            "Object" => "object",
            _ => symbol.Name
        };
    }

    public static bool IsCollection(this ITypeSymbol symbol, out string elementNamespace, out string elementName)
    {
        if (symbol is INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.IsGenericType)
            {
                foreach (var iface in namedTypeSymbol.Interfaces.Concat(new[] { namedTypeSymbol }))
                {
                    var name = iface.OriginalDefinition.ToString();

                    if (iface.ContainingNamespace.ToString() == "System.Collections.Generic")
                    {
                        if (name == "System.Collections.Generic.IList<T>" ||
                            name == "System.Collections.Generic.ICollection<T>" ||
                            name == "System.Collections.Generic.ISet<T>")
                        {
                            if (iface.TypeArguments.Length == 1)
                            {
                                var elementSymbol = iface.TypeArguments[0];
                                elementNamespace = elementSymbol.ContainingNamespace.ToString();
                                elementName = elementSymbol.ToSimplifiedString();
                                return true;
                            }
                        }
                    }
                }
            }
        }
        else if (symbol is IArrayTypeSymbol arrayTypeSymbol)
        {
            elementNamespace = arrayTypeSymbol.ElementType.ContainingNamespace.Name;
            elementName = arrayTypeSymbol.ElementType.ToSimplifiedString();
            return true;
        }

        elementNamespace = null;
        elementName = null;
        return false;
    }
    public static bool IsHashSet(this ITypeSymbol symbol)
    {
        if (symbol == null)
        {
            return false;
        }

        return symbol.AllInterfaces
            .Append(symbol)
            .OfType<INamedTypeSymbol>()
            .Any(iface => iface.Arity == 1 && iface.ConstructedFrom.ToString() == "System.Collections.Generic.ISet<T>");
    }

    public static bool IsObjectModelCollection(this ITypeSymbol symbol)
    {
        if (symbol == null)
        {
            return false;
        }

        if (symbol is INamedTypeSymbol nts)
        {
            if (nts.Arity == 1 && nts.ConstructedFrom.ToString() == "System.Collections.ObjectModel.Collection<T>")
            {
                return true;
            }
        }

        return false;
    }
    public static bool IsDictionary(this ITypeSymbol symbol)
    {
        if (symbol == null)
        {
            return false;
        }

        return symbol.AllInterfaces
            .Append(symbol)
            .OfType<INamedTypeSymbol>()
            .Any(iface => iface.Arity == 2 && iface.ConstructedFrom.ToString() == "System.Collections.Generic.IDictionary<TKey, TValue>");
    }
}
