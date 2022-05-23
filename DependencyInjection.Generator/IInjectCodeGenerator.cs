using Microsoft.CodeAnalysis;

namespace Dncy.MicrosoftDependencyInjection.Generator;

internal interface IInjectCodeGenerator
{
    string Generate(INamedTypeSymbol type);
    string GenerateWithInterface(INamedTypeSymbol type, INamedTypeSymbol interfaceType);
}