using System;
using Dncy.DependencyInjection.Generator.InjectCodeGenerators;

namespace Dncy.DependencyInjection.Generator;

internal class InjectCodeGeneratorFactory
{
    internal static IInjectCodeGenerator GetGenerator(EnumInjectLifeTime lifeTime)
    {
        switch (lifeTime)
        {
            case EnumInjectLifeTime.Scoped:
                return ScopedGenerator.Instrance;
            case EnumInjectLifeTime.Transient:
                return TransientGenerator.Instrance;
            case EnumInjectLifeTime.Singleton:
                return SingletonGenerator.Instrance;
            default: throw new ArgumentException("不支持的注入生命周期类型");
        }
    }



    internal enum EnumInjectLifeTime
    {
        Scoped = 0x01,
        Singleton = 0x02,
        Transient = 0x03
    }
}