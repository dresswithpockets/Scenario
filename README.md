# Scenario

Scenario is a small tool to help you write better, more maintainable, more reliable unit tests. While Scenario is designed for common use cases found when writing unit tests, it does not have any dependencies to any testing framework. If you find a good use outside of unit tests for Scenario, please let us know!

Scenario is *NOT* a unit testing framework. It is intended to be used alongside a unit testing framework like xUnit or NUnit; however, it may replace common features among some of these frameworks, such as class fixtures.

Scenario and its extensions are available as nuget packages:

- [Scenario](https://www.nuget.org/packages/Scenario)
- [Scenario.EFCore](https://www.nuget.org/packages/Scenario.EFCore)
- [Scenario.FakeS3](https://www.nuget.org/packages/Scenario.FakeS3)
- [Scenario.Moq](https://www.nuget.org/packages/Scenario.Moq)

Please checkout [the documentation](https://dresswithpockets.github.io/Scenario) to learn more about Scenario and how to use it.

## Why?

Writing tests can be really tedious and repetitive, and often difficult to maintain regularly, particularly within codebases that are in flux.

Scenario is a simple framework intended to reduce that friction as much as possible. It's got most of the boilerplate so you don't have to write it yourself.

## Example Usage

```cs
var scenario = await new ScenarioBuilder()
    .UseOnDiskDbContext<MyDbContext>()
    .WithMigrations<MyDbContext>()
    .BuildAsync();

using var scope = scenario.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();

var user = dbContext.Users.Find(userId);
Assert.NotNull(user);
```
