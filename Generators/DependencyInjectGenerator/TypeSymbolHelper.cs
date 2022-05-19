using System;
using Microsoft.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection.Generator;

internal static class TypeSymbolHelper
{
    public static string GetFullQualifiedName(this ISymbol symbol)
    {
        var containingNamespace = symbol.ContainingNamespace;
        if (!containingNamespace.IsGlobalNamespace)
            return containingNamespace.ToDisplayString() + "." + symbol.Name;

        return symbol.Name;
    }
}
