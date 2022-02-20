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
                foreach (var iface in namedTypeSymbol.Interfaces)
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

        elementNamespace = null;
        elementName = null;
        return false;
    }
}
