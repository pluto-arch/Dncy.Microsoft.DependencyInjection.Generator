using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Dncy.DependencyInjection.Generator.InjectCodeGenerators
{
    internal class CodeGeneratorBase : IInjectCodeGenerator
{
    public string Generate(INamedTypeSymbol type)
    {
        if (string.IsNullOrEmpty(LifeTimeMethod))
        {
            throw new InvalidOperationException("no life time found");
        }
        if (!type.IsGenericType)
        {
            return $@"service.{LifeTimeMethod}<{type.GetFullQualifiedName()}>();";
        }

        var typePa = type.TypeArguments;
        var typeParames = new List<string>();
        foreach (var item in typePa)
        {
            typeParames.Add(item.GetFullQualifiedName());
        }

        return $@"service.{LifeTimeMethod}<{type.GetFullQualifiedName()}<{string.Join(",", typeParames)}>>();";
    }


    public string GenerateWithInterface(INamedTypeSymbol type, INamedTypeSymbol interfaceType)
    {
        if (!type.IsGenericType && !interfaceType.IsGenericType)
        {
            return $@"service.{LifeTimeMethod}<{interfaceType.GetFullQualifiedName()},{type.GetFullQualifiedName()}>();";
        }

        if (interfaceType.IsGenericType && type.IsGenericType)
        {
            var typeArguments = interfaceType.TypeArguments;
            var commas = typeArguments.Length - 1;
            var s = string.Empty;
            for (int i = 0; i < commas; i++)
            {
                s += ",";
            }

            return $@"service.{LifeTimeMethod}(typeof({interfaceType.GetFullQualifiedName()}<{s}>),typeof({type.GetFullQualifiedName()}<{s}>));";
        }


        if (interfaceType.IsGenericType && !type.IsGenericType)
        {
            var typePa = interfaceType.TypeArguments;
            var typeParames = new List<string>();
            foreach (var item in typePa)
            {
                typeParames.Add(item.GetFullQualifiedName());
            }
            return $@"service.{LifeTimeMethod}<{interfaceType.GetFullQualifiedName()}<{string.Join(",", typeParames)}>,{type.GetFullQualifiedName()}>();";
        }



        return null;
    }



    public virtual string LifeTimeMethod { get; }
}
}

