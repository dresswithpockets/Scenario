# Scenario
Fluent API for dependency injection & mocking utilities for tests in .NET.

### Why?

Writing tests can get really tedious and repetitive. Scenario is a simple framework for reducing repition as much as possible when writing tests.

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
