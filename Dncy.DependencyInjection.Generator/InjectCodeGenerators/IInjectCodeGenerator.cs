using Microsoft.CodeAnalysis;

namespace Dncy.DependencyInjection.Generator.InjectCodeGenerators
{
    public interface IInjectCodeGenerator
    {
        string Generate(INamedTypeSymbol type);
        string GenerateWithInterface(INamedTypeSymbol type, INamedTypeSymbol interfaceType);
    }
}

