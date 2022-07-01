using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Dncy.DependencyInjection.Generator;

 [Generator]
    public class NativeDependencyInjectGenerator : ISourceGenerator
    {
        /// <inheritdoc />
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new TypeSyntaxReceiver());
        }

        /// <inheritdoc />
        public void Execute(GeneratorExecutionContext context)
        {
            var compilation = context.Compilation;
            var @namespace = "Microsoft.Extensions.DependencyInjection";
            var injectCodeNamespace = compilation.Assembly.Name;

            var syntaxReceiver = (TypeSyntaxReceiver)context.SyntaxReceiver;
            var injectTargets = syntaxReceiver?.TypeDeclarationsWithAttributes;

            if (injectTargets == null || !injectTargets.Any())
            {
                return;
            }

            var injectCodeStr = GeneratorInjectAttributeCode(context, @namespace);

            var options = (CSharpParseOptions)compilation.SyntaxTrees.First().Options;
            var logSyntaxTree = CSharpSyntaxTree.ParseText(injectCodeStr, options);
            compilation = compilation.AddSyntaxTrees(logSyntaxTree);
            var attribute = compilation.GetTypeByMetadataName($"{@namespace}.InjectableAttribute");
            var targetTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

            foreach (var targetTypeSyntax in injectTargets)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                var semanticModel = compilation.GetSemanticModel(targetTypeSyntax.SyntaxTree);
                var targetType = semanticModel.GetDeclaredSymbol(targetTypeSyntax);
                if (targetType?.DeclaredAccessibility!=Accessibility.Public)
                {
                    continue;
                }

                var hasInjectAttribute = targetType?.GetAttributes().Any(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, attribute)) ?? false;
                if (!hasInjectAttribute)
                    continue;
                targetTypes.Add(targetType);
            }


            try
            {
                var injectStr = $@" 
using Microsoft.Extensions.DependencyInjection;

namespace {injectCodeNamespace} {{
    public static class AutoInjectHelper
    {{
        public static IServiceCollection AutoInject{injectCodeNamespace.Replace(".", "_")}(this IServiceCollection service)
        {{";
                var sb = new StringBuilder(injectStr);

                foreach (var targetType in targetTypes)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();
                    var proxySource = GenerateInjectCode(targetType, @namespace, attribute);
                    sb.AppendLine(proxySource);
                }

                var end = $@"  return  service; }}
    }}
}}";
                sb.Append(end);

                context.AddSource($"AutoInjectHelper.Inject.cs", sb.ToString());
            }
            catch (Exception ex)
            {
                var desc = new DiagnosticDescriptor("GID002", ex.Message, ex.ToString(), "DI.Generate", DiagnosticSeverity.Error, true);
                context.ReportDiagnostic(Diagnostic.Create(desc, Location.None));
            }
        }

        private string GenerateInjectCode(ITypeSymbol targetType, string @namespace, INamedTypeSymbol attribute)
        {
            var attributeValue = targetType.GetAttributes().FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, attribute));
            if (attributeValue == null)
            {
                return null;
            }

            var attrobuitePropertys = attributeValue.ConstructorArguments;
            var typeEnum = attrobuitePropertys.FirstOrDefault(x => x.Kind == TypedConstantKind.Enum).Value;
            var types = attrobuitePropertys.FirstOrDefault(x => x.Kind == TypedConstantKind.Type).Value as ITypeSymbol;

            if (types == null)
            {
                return AddClassic(targetType, typeEnum);
            }
            else
            {
                return AddWithInterface(targetType, types, typeEnum);
            }

        }

        private string AddWithInterface(ITypeSymbol impl, ITypeSymbol @interface, object typeEnum)
        {
            switch (typeEnum)
            {
                case 0x01:
                    return GetInjectCodeWithInterfact(impl, @interface, InjectCodeGeneratorFactory.EnumInjectLifeTime.Scoped);
                case 0x02:
                    return GetInjectCodeWithInterfact(impl, @interface, InjectCodeGeneratorFactory.EnumInjectLifeTime.Singleton);
                case 0x03:
                    return GetInjectCodeWithInterfact(impl, @interface, InjectCodeGeneratorFactory.EnumInjectLifeTime.Transient);
                default:
                    return null;
            }
        }



        private string GetInjectCodeWithInterfact(ITypeSymbol implType, ITypeSymbol interfaceType, InjectCodeGeneratorFactory.EnumInjectLifeTime lifeTime)
        {
            if (implType == null)
                return null;
            return InjectCodeGeneratorFactory.GetGenerator(lifeTime).GenerateWithInterface((INamedTypeSymbol)implType, (INamedTypeSymbol)interfaceType);
        }


        private string AddClassic(ITypeSymbol name, object typeEnum)
        {
            switch (typeEnum)
            {
                case 0x01:
                    return GetInjectCode(name, InjectCodeGeneratorFactory.EnumInjectLifeTime.Scoped);
                case 0x02:
                    return GetInjectCode(name, InjectCodeGeneratorFactory.EnumInjectLifeTime.Singleton);
                case 0x03:
                    return GetInjectCode(name, InjectCodeGeneratorFactory.EnumInjectLifeTime.Transient);
                default:
                    return null;
            }
        }


        private string GetInjectCode(ITypeSymbol implType, InjectCodeGeneratorFactory.EnumInjectLifeTime lifeTime)
        {
            if (implType == null)
                return null;
            return InjectCodeGeneratorFactory.GetGenerator(lifeTime).Generate((INamedTypeSymbol)implType);
        }

        
        private string GeneratorInjectAttributeCode(GeneratorExecutionContext context, string @namespace)
        {
            var injectCodeStr = $@"
namespace {@namespace}
{{
    internal enum InjectLifeTime
    {{
        Scoped=0x01,
        Singleton=0x02,
        Transient=0x03
    }}

    [System.AttributeUsage(System.AttributeTargets.Class)]
    internal class InjectableAttribute:System.Attribute
    {{ 
        public InjectableAttribute(InjectLifeTime lifeTime,System.Type interfactType=null)
        {{
            LifeTime = lifeTime;
            InterfactType=interfactType;
        }}
		        
        public InjectLifeTime LifeTime {{get;}}

        public System.Type InterfactType {{get;}}
    }}
}}";

            context.AddSource("AutoInjectAttribute.g.cs", injectCodeStr);
            return injectCodeStr;
        }

    }