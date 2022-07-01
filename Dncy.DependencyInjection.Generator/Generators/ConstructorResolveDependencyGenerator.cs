using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Dncy.DependencyInjection.Generator.Generators;


[Generator]
public class ConstructorResolveDependencyGenerator : ISourceGenerator
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
        var defaultNameSpace = compilation.Assembly.Name;
        var syntaxReceiver = (TypeSyntaxReceiver)context.SyntaxReceiver;
        var injectTargets = syntaxReceiver?.TypeDeclarationsWithAttributes;

        if (injectTargets == null || !injectTargets.Any())
        {
            return;
        }
        var options = (CSharpParseOptions)compilation.SyntaxTrees.First().Options;
        var injectCodeStr = GeneratorTargetTypeAttributeCode(context, defaultNameSpace);
        var targetClassSyntaxTree = CSharpSyntaxTree.ParseText(injectCodeStr, options);
        compilation = compilation.AddSyntaxTrees(targetClassSyntaxTree);
        var targetClassAttribute = compilation.GetTypeByMetadataName($"{defaultNameSpace}.AutoResolveDependencyAttribute");
        var targetFieldAttribute = compilation.GetTypeByMetadataName($"{defaultNameSpace}.AutoInjectAttribute");

        var targetTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

        foreach (var targetTypeSyntax in injectTargets)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            var semanticModel = compilation.GetSemanticModel(targetTypeSyntax.SyntaxTree);
            var targetType = semanticModel.GetDeclaredSymbol(targetTypeSyntax);
            if (targetType?.DeclaredAccessibility != Accessibility.Public)
            {
                continue;
            }

            var hasInjectAttribute = targetType?.GetAttributes().Any(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, targetClassAttribute)) ?? false;
            if (!hasInjectAttribute)
                continue;
            targetTypes.Add(targetType);
        }

        if (!targetTypes.Any())
        {
            return;
        }

        var sb = new StringBuilder();
        var fields = new HashSet<(string, string)>(new FieldComparer());

        try
        {
            foreach (var typeSymbol in targetTypes)
            {
                var dependens = typeSymbol.GetMembers().Where(x => x.Kind == SymbolKind.Field);
                if (!dependens.Any())
                {
                    continue;
                }
                foreach (var memSymbol in dependens)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();
                    if (memSymbol is IFieldSymbol field)
                    {
                        var hasInjectAttribute = field?.GetAttributes().Any(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, targetFieldAttribute)) ?? false;
                        if (hasInjectAttribute)
                        {
                            fields.Add(($"{field.Type.ToString()} _{field.Name}", field.Name));
                        }
                    }
                }

                if (!fields.Any())
                {
                    continue;
                }

                GenerateDefaultConstructorInject(context, typeSymbol, sb, fields,targetFieldAttribute);
                sb.Clear();
                fields.Clear();
            }
        }
        catch (Exception ex)
        {
            sb.Clear();
            fields.Clear();
            var desc = new DiagnosticDescriptor("GID001", ex.Message, ex.ToString(), "DI.Generate", DiagnosticSeverity.Error, true);
            context.ReportDiagnostic(Diagnostic.Create(desc, Location.None));
        }
        finally
        {
            sb.Clear();
            fields.Clear();
        }
    }

    private static void GenerateDefaultConstructorInject(GeneratorExecutionContext context, ITypeSymbol typeSymbol, StringBuilder sb, HashSet<(string, string)> fields,INamedTypeSymbol targetFieldAttribute)
    {
        var typeName = typeSymbol.Name;
        var @namespace = typeSymbol.GetNameSpzce();
        bool hasBaseClas = false;
        var baseFields = new HashSet<string>();
        if (typeSymbol.BaseType is {TypeKind: TypeKind.Class, Constructors: { Length : > 0}})
        {
            hasBaseClas= true;
            var baset= typeSymbol.BaseType;
            var dependens = baset.GetMembers().Where(x => x.Kind == SymbolKind.Field);
            foreach (var memSymbol in dependens)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                if (memSymbol is IFieldSymbol field)
                {
                    var hasInjectAttribute = field?.GetAttributes().Any(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, targetFieldAttribute)) ?? false;
                    if (hasInjectAttribute)
                    {
                        var el = ($"{field.Type.ToString()} _{field.Name}", field.Name);
                        fields.Add(el);
                        baseFields.Add($"_{field.Name}");
                    }
                }
            }
        }


        sb.AppendLine($@"namespace {@namespace}");
        sb.AppendLine("{");
        sb.AppendLine($@"public partial class  {typeName}");
        sb.AppendLine("{");
        sb.AppendLine($@"public {typeName}(");
        sb.Append(string.Join(",", fields.Select(x => x.Item1)));
        sb.AppendLine($@")");

        if (hasBaseClas)
        {
            sb.AppendLine($@":base(");
            sb.Append(string.Join(",", baseFields.Select(x => x)));
            sb.AppendLine($@")");
        }
      

        sb.AppendLine("{");
        foreach (var memSymbol in fields)
        {
            if (!baseFields.Contains($"_{memSymbol.Item2}"))
            {
                sb.Append($"{memSymbol.Item2}=_{memSymbol.Item2};");
            }
        }

        sb.AppendLine("}");
        sb.AppendLine("}");
        sb.AppendLine("}");
        context.AddSource($"{@namespace}.{typeName}Inject.g.cs", sb.ToString());
    }

    private string GeneratorTargetTypeAttributeCode(GeneratorExecutionContext context, string defaultNameSpace)
    {
        var injectCodeStr = $@"
namespace {defaultNameSpace}
{{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    internal class AutoResolveDependencyAttribute:System.Attribute
    {{ 
    }}

    [System.AttributeUsage(System.AttributeTargets.Field|System.AttributeTargets.Property)]
    internal class AutoInjectAttribute:System.Attribute
    {{ 
    }}

}}";
        context.AddSource("ResolveDependencyAttribute.g.cs", injectCodeStr);
        return injectCodeStr;
    }



    private class FieldComparer:IEqualityComparer<(string,string)>
    {
        public bool Equals((string, string) x, (string, string) y)
        {
            return x.Item1 == y.Item1 && x.Item2 == y.Item2;
        }

        public int GetHashCode((string, string) obj)
        {
            unchecked
            {
                return (obj.Item1.GetHashCode() * 397) ^ obj.Item2.GetHashCode();
            }
        }
    }
}