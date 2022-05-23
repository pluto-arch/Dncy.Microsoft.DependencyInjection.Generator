using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Dncy.MicrosoftDependencyInjection.Generator.InjectCodeGenerators;

internal class ScopedGenerator : CodeGeneratorBase
{
    private static readonly Lazy<ScopedGenerator> _generator = new Lazy<ScopedGenerator>(() => new ScopedGenerator());

    public static ScopedGenerator Instrance = _generator.Value;

    private ScopedGenerator() { }

    /// <inheritdoc />
    public override string LifeTimeMethod => "AddScoped";
}
