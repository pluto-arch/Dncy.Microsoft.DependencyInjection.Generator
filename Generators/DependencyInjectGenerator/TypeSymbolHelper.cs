using System;
using Microsoft.CodeAnalysis;

namespace DependencyInjectGenerator;

public static class TypeSymbolHelper
{
    public static string GetFullQualifiedName(this ISymbol symbol)
    {
        var containingNamespace = symbol.ContainingNamespace;
        if (!containingNamespace.IsGlobalNamespace)
            return containingNamespace.ToDisplayString() + "." + symbol.Name;

        return symbol.Name;
    }
}
