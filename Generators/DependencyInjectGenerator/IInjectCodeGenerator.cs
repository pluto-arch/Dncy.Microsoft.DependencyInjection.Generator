using Microsoft.CodeAnalysis;

namespace DependencyInjectGenerator;

public interface IInjectCodeGenerator
{
    string Generate(INamedTypeSymbol type);
    string GenerateWithInterface(INamedTypeSymbol type, INamedTypeSymbol interfaceType);
}