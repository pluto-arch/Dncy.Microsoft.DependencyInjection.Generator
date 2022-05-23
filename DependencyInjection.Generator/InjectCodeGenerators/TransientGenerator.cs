using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Dncy.MicrosoftDependencyInjection.Generator.InjectCodeGenerators;

internal class TransientGenerator : CodeGeneratorBase
{
    private static readonly Lazy<TransientGenerator> _generator = new Lazy<TransientGenerator>(() => new TransientGenerator());

    public static TransientGenerator Instrance = _generator.Value;

    private TransientGenerator() { }


    /// <inheritdoc />
    public override string LifeTimeMethod => "AddTransient";
}