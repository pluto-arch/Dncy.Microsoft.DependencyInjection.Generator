# 依赖注入代码自动生成器

[nuget包](https://www.nuget.org/packages/Dncy.Microsoft.DependencyInjection.Generator/)

使用`Microsoft.Extensions.DependencyInjection`时，可能需要批量注入某些服务，比较普遍的做法时使用反射扫描程序集然后注入，为了0反射。使用source generator进行自动生成注入的代码。

## 使用方式

1. 安装nuget
```
Install-Package Dncy.Microsoft.DependencyInjection.Generator -Version 0.0.1
```

2. 在需要生成注入代码的类上使用特性：`InjectableAttribute`

`InjectableAttribute` 的参数：
```
/// <summary>
/// </summary>
/// <param name="lifeTime">生命周期，可选的值：Scoped、Singleton、Transient</param>
/// <param name="interfactType">使用接口解析的时候的接口类型</param>
public InjectableAttribute(InjectLifeTime lifeTime,Type interfactType=null)
{
}
```

3. 使用DI容器调用

所有AutoInject的开头的扩展就是自动生成的注入代码。默认的后缀是使用对应程序集名称，如果程序集包含"."。则被替换成"_"。

```
services.AutoInject<AssemblyName>();
```

