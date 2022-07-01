using System;

namespace Dncy.DependencyInjection.Generator.InjectCodeGenerators;

internal class TransientGenerator : CodeGeneratorBase
{
    private static readonly Lazy<TransientGenerator> _generator = new Lazy<TransientGenerator>(() => new TransientGenerator());

    public static TransientGenerator Instrance = _generator.Value;

    private TransientGenerator() { }


    /// <inheritdoc />
    public override string LifeTimeMethod => "AddTransient";
}