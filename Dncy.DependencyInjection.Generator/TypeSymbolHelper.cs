using Microsoft.CodeAnalysis;

namespace Dncy.DependencyInjection.Generator
{
    internal static class TypeSymbolHelper
    {
        public static string GetFullQualifiedName(this ISymbol symbol)
        {
            var containingNamespace = symbol.ContainingNamespace;
            if (!containingNamespace.IsGlobalNamespace)
                return containingNamespace.ToDisplayString() + "." + symbol.Name;

            return symbol.Name;
        }

    
        public static string GetNameSpzce(this ISymbol symbol)
        {
            var containingNamespace = symbol.ContainingNamespace;
            if (!containingNamespace.IsGlobalNamespace)
                return containingNamespace.ToDisplayString();

            return symbol.Name;
        }
    }

}

