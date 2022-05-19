using Microsoft.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection.Generator;

internal interface IInjectCodeGenerator
{
    string Generate(INamedTypeSymbol type);
    string GenerateWithInterface(INamedTypeSymbol type, INamedTypeSymbol interfaceType);
}