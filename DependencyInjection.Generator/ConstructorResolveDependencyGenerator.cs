using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Dncy.MicrosoftDependencyInjection.Generator.SyntaxReceivers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Dncy.MicrosoftDependencyInjection.Generator;

[Generator]
public class ConstructorResolveDependencyGenerator: ISourceGenerator
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

            if (targetType?.DeclaredAccessibility!=Accessibility.Public)
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
        var fields = new List<(string,string)>();

       

        try
        {
            foreach (var typeSymbol in targetTypes)
            {
                var dependens = typeSymbol.GetMembers().Where(x=>x.Kind==SymbolKind.Field);
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
                            fields.Add(($"{field.Type.ToString()} _{field.Name}",field.Name));
                        }
                    }
                }

                if (!fields.Any())
                {
                    continue;
                }

                GenerateDefaultConstructorInject(context, typeSymbol, sb, fields);

                sb.Clear();
                fields.Clear();
            }
        }
        catch (Exception)
        {
            sb.Clear();
            fields.Clear();
        }
        finally
        {
            sb.Clear();
            fields.Clear();
        }



    }

    private static void GenerateDefaultConstructorInject(GeneratorExecutionContext context, ITypeSymbol typeSymbol,
        StringBuilder sb, List<(string, string)> fields)
    {
        var typeName = typeSymbol.Name;
        sb.AppendLine($@"namespace {typeSymbol.GetNameSpzce()}");
        sb.AppendLine("{");
        sb.AppendLine($@"public partial class  {typeName}");
        sb.AppendLine("{");
        sb.AppendLine($@"public {typeName}(");
        sb.Append(string.Join(",", fields.Select(x => x.Item1)));
        sb.AppendLine($@")");
        sb.AppendLine("{");
        foreach (var memSymbol in fields)
        {
            sb.Append($"{memSymbol.Item2}=_{memSymbol.Item2};");
        }

        sb.AppendLine("}");
        sb.AppendLine("}");
        sb.AppendLine("}");
        context.AddSource($"{typeName}Inject.g.cs", sb.ToString());
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
}