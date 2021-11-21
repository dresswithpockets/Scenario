# Source Generation / Making Scenario nicer to work with

v0.2 introduced a new feature that makes working with Scenario a lot nicer on the finger tips: Source generation.

Before v0.2, if you wanted to write builder extensions, they would look something like:

```c#
public static class ScenarioUserExtensions
{
    public static TScenarioBuilder UseUsers<TScenarioBuilder>(this TScenarioBuilder builder)
        where TScenarioBuilder : IScenarioBuilder
        => (TScenarioBuilder) builder.Use(services => services.AddTransient<IUserService, UserService>());
    
    public static TScenarioBuilder WithUser<TScenarioBuilder>(this TScenarioBuilder builder, Action<User>? resultCallback = null)
        where TScenarioBuilder : IScenarioBuilder
        => (TScenarioBuilder) builder.With(
                async scope => await scope.ServiceProvider.GetRequiredService<IUserService>().CreateUserAsync(),
                u => user = (User)u);
}
```

Now, with the `Scenario.SourceGenerator` package, you can rewrite them to look like:

```c#
public static class ScenarioUserExtensions
{
    [ScenarioDependency]
    public static void UseUsers(IServiceCollection services)
        => services.AddTransient<IUserService, UserService>();

    [ScenarioResource]
    public static async Task<User> WithUser(IServiceScope scope)
        => await scope.ServiceProvider.GetRequiredService<IUserService>().CreateUserAsync();
}
```

N.B. this only works with C#.

## Getting started

Install Scenario.SourceGenerator from [Nuget](https://www.nuget.org/packages/Scenario.SourceGenerator):

```
dotnet add package Scenario.SourceGenerator
```

which will add itself as a code analyzer to your project.

## How it works

The source generator analyzes your C# source code & looks for public static methods with one of the `ScenarioDependency` or `ScenarioResource` attributes. These attributes are added to your project's source imperatively by the source generator.

Lets go into more details about how the source generator analyzes methods with each attribute

There are a few requirements for your method to be a valid extension method:

- It must be public and static.
- If it is a Dependency, it must return void.
- If it is a Dependency, it must always accept an `IServiceCollection` as its first parameter, even if it does not utilize it.
- If it is a Resource, it must return a `Task` or `Task<T>`.
- If it is a Resource, it must always accept an `IServiceScope` as its first parameter, even if it does not utilize it.
- It must not accept a `ref` or `out` parameter.
- It must not have a type generic parameter named `__TScenarioBuilder`
    - This type parameter is used by the generated C# code
- It must not have a function parameter named `__scenarioBuilder`, `__services`, or `__scope`
    - These parameter names are used by the generated C# code

If each of these requirements are met, the source generator will generate a valid extension method for ScenarioBuilder that invokes your static function. On top of these requirements are some allowances for your function:

- It may accept any additional non-ref and non-out parameters.
- It may accept a params parameter.
- It may accept default values for those parameters.
- It may accept any additional generic type arguments.
- It may have additional generic type constraints.
- It may have no body - such as an `extern` or `partial` function.

The source generator has considerations and logic for each of these requirements and allowances. If one is not listed here, it may not be directly supported - please post an issue if that is the case and you would like a consideration for such a case.

### ScenarioDependencyAttribute

This attribute signals a method which may add a service to the service collection, or do any synchronous action which does not depend on any service within the scenario thus far. Analogous to a `Use` method, i.e `UseInMemoryS3`.

For example, here is an example of a dependency method which adds a localhost Postgres EFCore DbContext to the service collection, with a parameter accepting the name of the migrations assembly:

```c#
public static class ScenarioExtensions
{
    [ScenarioDependency]
    public static void UseNpgsql<TDbContext>(IServiceCollection services, string migrationsAssembly = "MyLib.Migrations")
        where TDbContext : DbContext
        => services.AddDbContext<TDbContext>(options => 
            options.UseNpgsql("host=localhost;db=postgres;user=postgres;pass=postgres", x => x.MigrationsAssembly(migrationsAssembly)));
}
```

Which will trigger the source generator to generate an extension method like this:

```c#
public static __TScenarioBuilder UseNpgsql<__TScenarioBuilder, TDbContext>(this __TScenarioBuilder __scenarioBuilder,
                                                                           string migrationsAssembly = "MyLib.Migrations")
    where __TScenarioBuilder : IScenarioBuilder
    where TDbContext : DbContext
    => (__TScenarioBuilder) __scenarioBuilder.Use(services => ScenarioExtensions.UseNpgsql<TDbContext>(services, migrationsAssembly));
```

Which you may use like so:

```c#
var scenario = await new ScenarioBuilder()
                .UseNpgsql<ScenarioBuilder, MyDbContext>()
                .BuildAsync();

using var scope = scenario.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
```

### ScenarioResourceAttribute

This attribute signals a method which will do some processing which may depends on a service within the scenario's service collection. Analogous to a `With` method, i.e `WithUser`.

### Changing the extension's name

Each of the aforementioned attributes has an `ExtensionName` property that will override the name of the attribute, like so:

```c#
[ScenarioDependency(ExtensionName = "UseThing")]
public static void ConfigureThing(IServiceCollection services)
    => // ...
```

which you can use like so:

```c#
new ScenarioBuilder()
    .UseThing()
```

### Generating a non-TScenarioBuilder-generic version of the extension

Lets take this sample from earlier:

```c#
var scenario = await new ScenarioBuilder()
                .UseNpgsql<ScenarioBuilder, MyDbContext>()
                .BuildAsync();
```

C# doesn't support partial type inference in generic type parameters, so you have to provide a type for every type parameter, including the `TScenarioBuilder` parameter.

To get around this, we can create a non-generic form of the extension. We can trigger the source generator to generate both the normal generic form as well as this form of the extension, with the `CreateNonGenericExtension` flag:

```c#
[ScenarioDependency(CreateNonGenericExtension = true)]
public static void UseNpgsql<TDbContext>(IServiceCollection services, string migrationsAssembly = "MyLib.Migrations")
    // ...
```

which will generate two extension methods, one in the generic form and one in the not-generic form:

```c#
public static __TScenarioBuilder UseNpgsql<__TScenarioBuilder, TDbContext>(this __TScenarioBuilder __scenarioBuilder,
                                                                           string migrationsAssembly = "MyLib.Migrations")
    where __TScenarioBuilder : IScenarioBuilder
    where TDbContext : DbContext
    => (__TScenarioBuilder) __scenarioBuilder.Use(services => ScenarioExtensions.UseNpgsql<TDbContext>(services, migrationsAssembly));

public static IScenarioBuilder UseNpgsql<TDbContext>(this IScenarioBuilder __scenarioBuilder,
                                                     string migrationsAssembly = "MyLib.Migrations")
    where TDbContext : DbContext
    => __scenarioBuilder.Use(services => ScenarioExtensions.UseNpgsql<TDbContext>(services, migrationsAssembly));
```

allowing you to drop the first generic parameter:

```c#
var scenario = await new ScenarioBuilder
                .UseNpgsql<MyDbContext>()
                .BuildAsync();
```
