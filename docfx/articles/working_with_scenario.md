# Working With Scenario

Approx. 10 minute read.

This doc is a bit of a deeper dive with how each component in a typical Scenario works.

## Creating a scenario

Scenarios are, at their core, just a collection of services (dependencies) and data (resources). You may add to those dependencies and resources freely via `ScenarioBuilder`:

```cs
var builder = new ScenarioBuilder();
```

From here you may add a dependency necessary for the scenario - the `Use` method allows you to add services to the service collection, not dissimilar to `ConfigureServices(IServiceCollection)` in WebHost Startup classes:

```cs
builder.Use(services => services.AddTransient<IUserService, UserService>());
```

For the sake of our example, lets create a user given this hypothetical `IUserServices` - the `With` method takes a factory method which is passed an `IServiceScope`, exposing all of the dependencies we've added to the scenario thus far:

```cs
builder.With(async scope => await scope.ServiceProvider.GetRequiredService<IUserService>().CreateUserAsync());
```

Now we've created a builder with an `IUserService` dependency, and a resource factory for creating a user; however, that factory hasn't actually executed yet. We need to build the scenario from the builder for that:

```cs
var scenario = await builder.BuildAsync();
```

If we put that all together, this is what we end up with:

```cs
var scenario = await new ScenarioBuilder()
    .Use(services => services.AddTransient<IUserService, UserService>())
    .With(async scope => await scope.ServiceProvider.GetRequiredService<IUserService>().CreateUserAsync())
    .BuildAsync();
```

## Using a scenario

Given what we have so far:

```cs
var scenario = await new ScenarioBuilder()
    .Use(services => services.AddTransient<IUserService, UserService>())
    .With(async scope => await scope.ServiceProvider.GetRequiredService<IUserService>().CreateUserAsync())
    .BuildAsync();
```

we've created a User resource - we may want to use that resource outside of the scenario and within our test. The quickest way to get access to the resource is via the result callback available as the 2nd parameter in the `With` function, which will be invoked just after the resource is created, with the result of the factory function.

```cs
User user = null!;
//...
    .With(async scope => await scope.ServiceProvider.GetRequiredService<IUserService>().CreateUserAsync(),
        u => user = (User)u);
//...
```

After `BuildAsync` gets called, the factory method gets invoked & the result of it gets passed into the callback, so by the time `BuildAsync` finishes, `user` will be occupied by the value established within the factory method. Lets validate this behaviour by taking out a scope on the scenario & doing an assertion based on the result of a hypothetical api in our hypothetical service:

```cs
using var scope = scenario.CreateScope();
var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

var retrievedUser = await userService.GetUserById(user!.Id);

Assert.NotNull(retirevedUser);
Assert.Equal(user.Id, retrievedUser.Id);
```

This ultimately asserts that `GetUserById` works as intended... its a unit test! Lets put it all together to get the bigger picture:

```cs
[Fact]
public async Task UserService_GetsUserById()
{
    // arrange
    User user = null!;
    var scenario = await new ScenarioBuilder()
        .Use(services => services.AddTransient<IUserService, UserService>())
        .With(async scope => await scope.ServiceProvider.GetRequiredService<IUserService>().CreateUserAsync(),
            u => user = (User)u);
        .BuildAsync();

    // act
    using var scope = scenario.CreateScope();
    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

    var retrievedUser = await userService.GetUserByIdAsync(user!.Id);

    // assert
    Assert.NotNull(retirevedUser);
    Assert.Equal(user.Id, retrievedUser.Id);
}
```

## Refactoring common utilities

Lets take a look at our current unit test:

```cs
[Fact]
public async Task UserService_GetsUserById()
{
    // arrange
    User user = null!;
    var scenario = await new ScenarioBuilder()
        .Use(services => services.AddTransient<IUserService, UserService>())
        .With(async scope => await scope.ServiceProvider.GetRequiredService<IUserService>().CreateUserAsync(),
            u => user = (User)u);
        .BuildAsync();

    // act
    using var scope = scenario.CreateScope();
    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

    var retrievedUser = await userService.GetUserByIdAsync(user!.Id);

    // assert
    Assert.NotNull(retirevedUser);
    Assert.Equal(user.Id, retrievedUser.Id);
}
```

This is pretty good; however, we might want to reuse some of the functionality we have here. Lets move our `Use` and `With` code into a couple common extension methods that we can call upon from within any unit test:

```cs
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

Thats is quite a bit more verbose; while we make that tradeoff, what we get back is reusability & simplifying our unit tests:

```cs
[Fact]
public async Task UserService_GetsUserById()
{
    // arrange
    User user = null!;
    var scenario = await new ScenarioBuilder()
        .UseUsers()
        .WithUser(u => user = u);
        .BuildAsync();

    // act
    using var scope = scenario.CreateScope();
    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

    var retrievedUser = await userService.GetUserByIdAsync(user!.Id);

    // assert
    Assert.NotNull(retirevedUser);
    Assert.Equal(user.Id, retrievedUser.Id);
}

// here is another hypotetical unit test using those extension methods we just wrote
[Fact]
public async Task UserService_GetsAllUsers()
{
    // arrange
    var users = new List<User>();
    var scenario = await new ScenarioBuilder()
        .UseUsers()
        .WithUser(u => users.Add(u))
        .WithUser(u => users.Add(u))
        .BuildAsync();

    // act
    using var scope = scenario.CreateScope();
    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

    var retrievedUsers = await userService.GetAllUsersAsync().ToImmutableList();

    // assert
    Assert.Equal(users.Count, retrievedUsers.Count)
    foreach (var retrievedUser in retrievedUsers)
        Assert.NotNull(users.SingleOrDefault(u => u.Id == retrievedUser.Id));
}
```
