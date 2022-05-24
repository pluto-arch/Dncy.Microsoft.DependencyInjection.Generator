# 基于Source Generator的原生依赖注入解析依赖的生成器

这里就不废话了，上一节中，我们增加了一个注入代码的生成器。用来注入用到的对象。
最佳的解析依赖的方式是使用构造函数解析。

- 新建生成器

```
[Generator]
public class ConstructorResolveDependencyGenerator: ISourceGenerator
{
     public void Initialize(GeneratorInitializationContext context){
        /// ... 省略     
     }

     public void Execute(GeneratorExecutionContext context){
         var compilation = context.Compilation;
        var defaultNameSpace = compilation.Assembly.Name;
        var syntaxReceiver = (TypeSyntaxReceiver)context.SyntaxReceiver;
        var injectTargets = syntaxReceiver?.TypeDeclarationsWithAttributes;

        if (injectTargets == null || !injectTargets.Any())
        {
            return;
        }
        
        
        var options = (CSharpParseOptions)compilation.SyntaxTrees.First().Options;
        
        // 这几行是还是生成标记类型，一个是标记这个类型是否支持自动解析
        // 另一个是标记这个类型的字段成员是否自动在构造函数中解析
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
            // 开始遍历目标类型 
            foreach (var typeSymbol in targetTypes)
            {
                // 获取字段成员
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
                        // 将字段成员转化为字段符号，然后获取特性标记
                        var hasInjectAttribute = field?.GetAttributes().Any(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, targetFieldAttribute)) ?? false;
                        if (hasInjectAttribute)
                        {
                            // 添加构造函数参数和赋值字段名称
                            fields.Add(($"{field.Type.ToString()} _{field.Name}",field.Name));
                        }
                    }
                }

                if (!fields.Any())
                {
                    continue;
                }

                // 生成每个类型的构造函数部分并注入依赖。
                GenerateDefaultConstructorInject(context, typeSymbol, sb, fields);

                sb.Clear();
                fields.Clear();
            }
        }
        catch (Exception)
        {
            sb.Clear();
            fields.Clear();
            // 这里的话 如果出现异常会在错误列表中输出对应的信息
             context.ReportDiagnostic(
                    Diagnostic.Create(
                        "AUTODI_02",
                        "ConstructorResolveDependency Generator",
                        $"生成注入代码失败，{e.Message}",
                        defaultSeverity: DiagnosticSeverity.Error,
                        severity: DiagnosticSeverity.Error,
                        isEnabledByDefault: true,
                        warningLevel: 0));
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
        // 这里目前必须使用分部类，对应用户代码中也需要使用分部类
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
        
        // 将生成的代码添加到上下文中
        context.AddSource($"{typeName}Inject.g.cs", sb.ToString());
    }
     
     
     
     
     private string GeneratorTargetTypeAttributeCode(GeneratorExecutionContext context, string defaultNameSpace)
    {
        var injectCodeStr = $@"
namespace {defaultNameSpace}
{{
    [System.AttributeUsage(AttributeTargets.Class)]
    internal class AutoResolveDependencyAttribute:System.Attribute
    {{ 
    }}

    [System.AttributeUsage(AttributeTargets.Field|AttributeTargets.Property)]
    internal class AutoInjectAttribute:System.Attribute
    {{ 
    }}

}}";
        context.AddSource("ResolveDependencyAttribute.g.cs", injectCodeStr);
        return injectCodeStr;
    }
     
     
}
```

以上就是自动生成类型构造函数并注入依赖的生成器。此生成器目前适用与比较简单的场景，有继承的并且继承父类中有构造或者父类也使用了自动解析的，会发生冲突，这就涉及在解析类型语法的时候同时需要解析父类，并且还需要分用户构造函数和自动生成的构造函数等不同情况。不过大多树简单场景下的解析是够用了。