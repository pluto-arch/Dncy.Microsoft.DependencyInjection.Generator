using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Dncy.MicrosoftDependencyInjection.Generator.InjectCodeGenerators;

internal class SingletonGenerator : CodeGeneratorBase
{

    private static readonly Lazy<SingletonGenerator> _generator = new Lazy<SingletonGenerator>(() => new SingletonGenerator());

    public static SingletonGenerator Instrance = _generator.Value;

    private SingletonGenerator() { }

    /// <inheritdoc />
    public override string LifeTimeMethod => "AddSingleton";
}