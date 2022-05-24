# 基于Source Generator的原生依赖注入代码生成器

## 简介和快速入门
具体介绍也不废话了，想了解的可以查看官方介绍：[源生成器](https://docs.microsoft.com/zh-cn/dotnet/csharp/roslyn-sdk/source-generators-overview)

## 原生注入

我们都知道原生的注入时使用`ServiceCollection`进行的。比如以下代码：
```csharp
var services = new ServiceCollection();
services.AddScoped<Demo>();
```
注入的生命周期有三种：`Singleton` 、`Scoped`、`Transient`
常见的注入方式有：
1. 直接注入具体对象
```csharp
services.AddScoped<Demo>();
services.AddScoped(typeof(Demo));
services.AddScoped(typeof(Demo<>)); // 泛型，多个泛型参数用<,>分割
```
2. 注入接口和其实现
```charp
services.AddScoped<SomeInterface,SomeImpl>(); // 支持注入多个不同的实现
services.AddScoped(typeof(SomeInterface),typeof(SomeImpl)); 
services.AddScoped(typeof(SomeInterface<>),typeof(SomeImpl<>)); 
```
以上就是很常见的注入方式。某些情况下我肯可能需要批量注入，比如有 AService、BService、CService...
传统做法是使用反射进行扫描批量注入。反射会有一定的性能影响。而通过一些静态织入的框架可以在编译成目标前进行代码切入，从而实现可以实现一些优雅的功能。比如一些AOP组件，颠覆了以前使用反射的动态代理方式。

## 原生注入代码生成器

大家看完官方的教程后应该都对Source Generator有了一些了解。那我们从0开始手写一个原生注入的生成器。

-  新建一个netstand2.0的项目
-  引入代码分析器和csharpo的分析器(其他语言也有自己的分析器)
在1创建好的项目的csproj中加入以下代码(或者使用nuget安装)
> 注意：Microsoft.CodeAnalysis.CSharp 暂时不要装最新的版本，好像有问题生成不了。
```xml
<ItemGroup>
		<PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.3">
			<PrivateAssets>all</PrivateAssets>
			<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
		</PackageReference>
		<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.0.1" PrivateAssets="all" />
	</ItemGroup>
```

-  新建语法接受器(Syntax Receiver)
这里我们需要分析所有的用户自定义类型。所以只需要接收语法中的类型就好了，不需要其他的。
语法接收器需要实现`ISyntaxReceiver`接口
```csharp
internal class TypeSyntaxReceiver : ISyntaxReceiver
{
    // 声明一个集合用来暂存接收到的类型语法
    public HashSet<TypeDeclarationSyntax> TypeDeclarationsWithAttributes { get; } = new();
    
    // 当编译器访问语法节点的时候会调用此方法
    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        // 这里判断这个节点是不是类型声明的语法并且是否包含特性声明语法。我们只对打了特殊标记的进行处理，其余的就不处理。减少一些编译时的影响
        if (syntaxNode is TypeDeclarationSyntax declaration && declaration.AttributeLists.Any())
        {
            TypeDeclarationsWithAttributes.Add(declaration);
        }
        
        // 还有比如：MethodDeclarationSyntax 方法声明 InterfaceDeclarationSyntax接口声明等等，根据需要可以接受不同的语法结点进行分析。
    }

}
```

以上我们就定义了一个语法接收器，用来接收编译器分析阶段的语法节点。

- 新建生成器
生成器需要实现`ISourceGenerator`接口和使用`Generator`标记
接口：`ISourceGenerator`有两个方法 `Initialize` 和 `Execute`，看名字我们就知道是干什么的了。

> 注意：生成的代码中用到的类型最好使用完成命名空间限定，使用using方式会比较复杂。解析的话会有一些考虑不全的场景。

```csharp
[Generator]
public class NativeDependencyInjectGenerator : ISourceGenerator
{
    // 初始化
    public void Initialize(GeneratorInitializationContext context)
    {
        if (!Debugger.IsAttached) // 这几句是附加调试的，加上这个会进入调试模式，和正常的c#代码调试一样。
        {
            Debugger.Launch();
        }
        // 上下文注册语法接收器
        context.RegisterForSyntaxNotifications(() => new TypeSyntaxReceiver());
    }
    
    // 具体的执行方法
    public void Execute(GeneratorExecutionContext context)
    {
        // 这里获取当前编译上下文的编译对象，只会包含用户代码
        var compilation = context.Compilation;
        // 获取命名空间 这里使用当前编译的程序集名称，还可以获取入口程序所在的名称空间。但是我们是直接生成这个程序集下的所有声明未可注入的，所以就直接用程序集名称了。
        var injectCodeNamespace = compilation.Assembly.Name;
        
        // 这两句是从上下文中拿到我们需要分析的类型语法分析器，并拿到类型集合。
        var syntaxReceiver = (TypeSyntaxReceiver)context.SyntaxReceiver;
        var injectTargets = syntaxReceiver?.TypeDeclarationsWithAttributes;
        if (injectTargets == null || !injectTargets.Any())
            return;
        
        // 这里是生成可注入的特性代码，并添加到编译上下文。
        // 这里生成的代码最终是一个特性，只能标记在public class上，打了此标记的才会生成到注入代码中
        var injectAttributeStr = GeneratorInjectAttributeCode(context, @namespace);
        
        // 这几行是将上边的特性添加到语法树，并获取原信息。后边要做处理
        var options = (CSharpParseOptions)compilation.SyntaxTrees.First().Options;
        var logSyntaxTree = CSharpSyntaxTree.ParseText(injectCodeStr, options);
        compilation = compilation.AddSyntaxTrees(logSyntaxTree);
        var attribute = compilation.GetTypeByMetadataName($"{@namespace}.InjectableAttribute");
        
        // 这里定义一个最终的分析类型集合，里面包含的就是有注入标记的类型。
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
            
                // 这里就是具体的注入代码生成的地方，基本都是拼接字符串。最终生成IServiceCollection的扩展。
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
            catch (Exception e)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        "AUTODI_01",
                        "DependencyInject Generator",
                        $"生成注入代码失败，{e.Message}",
                        defaultSeverity: DiagnosticSeverity.Error,
                        severity: DiagnosticSeverity.Error,
                        isEnabledByDefault: true,
                        warningLevel: 0));
            }
        
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

    [System.AttributeUsage(AttributeTargets.Class)]
    internal class InjectableAttribute:System.Attribute
    {{ 
        public InjectableAttribute(InjectLifeTime lifeTime,Type interfactType=null)
        {{
            LifeTime = lifeTime;
            InterfactType=interfactType;
        }}
		        
        public InjectLifeTime LifeTime {{get;}}

        public Type InterfactType {{get;}}
    }}
}}";
context.AddSource("AutoInjectAttribute.g.cs", injectCodeStr);
            return injectCodeStr;
        }
    
    
}
```

以上就是自动生成当前程序集里面可注入的对象的代码生成器，具体的细节代码请参看源码。