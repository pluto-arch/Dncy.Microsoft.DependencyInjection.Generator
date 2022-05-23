using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Dncy.AspNetCoreController.Generator.SyntaxReceivers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Dncy.AspNetCoreController.Generator
{
    [Generator]
    public class ControllerGenerator: ISourceGenerator
    {
        /// <inheritdoc />
        public void Initialize(GeneratorInitializationContext context)
        {
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
            context.RegisterForSyntaxNotifications(() => new ControllerSyntaxReceiver());
        }

        /// <inheritdoc />
        public void Execute(GeneratorExecutionContext context)
        {
            var compilation = context.Compilation;
            var defaultNameSpace = compilation.Assembly.Name;


            var syntaxReceiver = (ControllerSyntaxReceiver)context.SyntaxReceiver;
            var injectTargets = syntaxReceiver?.ControllerTypeDeclarationsWithAttributes;

            if (injectTargets == null || !injectTargets.Any())
            {
                return;
            }

            var injectCodeStr = GeneratorAutoControllerAttributeCode(context, defaultNameSpace);
            var options = (CSharpParseOptions)compilation.SyntaxTrees.First().Options;
            var logSyntaxTree = CSharpSyntaxTree.ParseText(injectCodeStr, options);
            compilation = compilation.AddSyntaxTrees(logSyntaxTree);

            var logAttribute = compilation.GetTypeByMetadataName($"{defaultNameSpace}.AutoControllerAttribute");
            var targetTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

            foreach (var targetTypeSyntax in injectTargets)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                var semanticModel = compilation.GetSemanticModel(targetTypeSyntax.SyntaxTree);
                var targetType = semanticModel.GetDeclaredSymbol(targetTypeSyntax);
                var hasInjectAttribute = targetType?.GetAttributes().Any(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, logAttribute)) ?? false;
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
                        if (memSymbol is IFieldSymbol field)
                        {
                            fields.Add(($"{field.Type.ToString()} _{field.Name}",field.Name));
                        }
                    }
                    var typeName = typeSymbol.Name.Replace("Controller", string.Empty);
                    sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
                    sb.AppendLine($@"namespace {typeSymbol.GetNameSpzce()}");
                    sb.AppendLine("{");
                    sb.AppendLine($@"public partial class  {typeName}Controller:ControllerBase");
                        sb.AppendLine("{");
                        sb.AppendLine($@"public {typeName}Controller(");
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
                    context.AddSource($"{typeName}Controller.g.cs", sb.ToString());
                    sb.Clear();
                    fields.Clear();
                }
            }
            catch (Exception e)
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

        private string GeneratorAutoControllerAttributeCode(GeneratorExecutionContext context, string defaultNameSpace)
        {
            var injectCodeStr = $@"
namespace {defaultNameSpace}
{{
    [System.AttributeUsage(AttributeTargets.Class)]
    internal class AutoControllerAttribute:System.Attribute
    {{ 
    }}
}}";
            context.AddSource("AutoControllerAttribute.g.cs", injectCodeStr);
            return injectCodeStr;
        }

    }
}