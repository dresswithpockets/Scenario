# Quickstart

Approx. 10 minute read.

This is a brief overview of the central concepts of Scenario and how to use Scenario in a unit test. This overview will also go over creating your own extensions for Scenario - necessary for creating maintainable and reusable components for your unit tests.

## Concepts

### Dependencies

Dependencies, aka Services, are objects added to the service collection that Scenario wraps. If you are familiar with writing services in ASP.NET projects, this is the same.

### Resources

Resources are objects created after the service collection has been built and a scope has been created with all of those dependencies. Usually one of those dependencies are used to create the resource. These are usually used in the same way as mocked objects or data in tests.

#### Dependent Resources

Dependent resources are resources that require another resource to be created.

## Getting Started

### Install

Install Scenario from [Nuget](https://www.nuget.org/packages/Scenario):

```
dotnet add package Scenario
```

### Basic Usage

Here is a naive example of how to build a Scenario with a prexisting service, and no extensions:

```cs
User user = null!;
var scenario = await new ScenarioBuilder()
    .Use(services => services.AddTransient<IUserService, UserService>())
    .With(scope => scope.GetRequiredService<IUserService>().CreateNewUser(), u => user = (User)u!);
    .BuildAsync();

using var scope = scenario.CreateScope();
var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

var users = userService.GetUsers();
Assert.NotNull(users);
```

Almost all operations will terminate on `ScenarioBuilder` in some way.

`Use` calls the given action with the `IServiceCollection` wrapped by `ScenarioBuilder`, allowing you to add [Dependencies](#Dependencies) to it just as you would in ASP.NET's `ConfigureServices` startup function.

`With` configures the builder to callback on the given [Resource](#Resources) factory method, and pass the result along to the callback if necessary. Factories configured via `With` are executed imperatively, in order, once `BuildAsync` is called.

`BuildAsync` builds final `IServiceProvider` from the builder's `IServiceCollection`, creates a new scope and passes that scope to each of the Resource factories in order by declaration order.

### Using Extensions

First, lets include some common extensions for interacting with EF Core DbContexts, which our hypothetical `UserService` may depend on:

Install Scenario.EFCore from [Nuget](https://www.nuget.org/packages/Scenario.EFCore/):

```
dotnet add package Scenario.EFCore
```

Lets create an extension for our user service Dependency:

```cs
public static TScenarioBuilder UseUsers<TScenarioBuilder>(
    this TScenarioBuilder scenarioBuilder)
    where TScenarioBuilder : IScenarioBuilder
    => (TScenarioBuilder) scenarioBuilder
        .Use(services => services.AddTransient<IUserService, UserService>());
```

And an extension for mocking an arbitrary user Resource:

```cs
public static TScenarioBuilder WithUser<TScenarioBuilder>(
    this TScenarioBuilder scenarioBuilder,
    string email,
    Action<User>? resultCallback = null)
    where TScenarioBuilder : IScenarioBuilder
    => (TScenarioBuilder) scenarioBuilder
        .With(scope => scope.ServiceProvider.GetRequiredService<IUserService>().CreateNewUser(email),
            u => resultCallback?.Invoke((User)u!));
```

And put it all to use in a hypothetical unit test:

```cs
const string testEmail = "test@test.com";
User user = null!;
var scenario = await new ScenarioBuilder()
    .UseOnDiskSqliteDbContext<MyDbContext>()
    .UseUsers()
    .WithUser(testEmail, u => user = u!);
    .BuildAsync();

using var scope = scenario.CreateScope();
var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

var foundUser = userService.GetUserByEmail(testEmail);
Assert.NotNull(foundUser);
Assert.Equal(user.Id, foundUser.Id);
Assert.Equal(user.Email, foundUser.Email);
```

Thats quite a bit nicer isn't it? Extensions are the heart of what make Scenario easy to work with. While writing extensions is itself tedious and boilerplate heavy; once you write one, it can be reused anywhere `ScenarioBuilder` is used, and will improve the test writing experience quite a bit! 

The hope is for Scenario to be a well-documented and well-maintained paradigm for writing consistent testing scenarios.