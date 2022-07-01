using System;

namespace Dncy.DependencyInjection.Generator.InjectCodeGenerators;

internal class ScopedGenerator : CodeGeneratorBase
{
    private static readonly Lazy<ScopedGenerator> _generator = new Lazy<ScopedGenerator>(() => new ScopedGenerator());

    public static ScopedGenerator Instrance = _generator.Value;

    private ScopedGenerator() { }

    /// <inheritdoc />
    public override string LifeTimeMethod => "AddScoped";
}