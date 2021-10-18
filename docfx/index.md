# Scenario

Scenario is a small tool to help you write better, more maintainable, more reliable unit tests. While Scenario is designed for common use cases found when writing unit tests, it does not have any dependencies to any testing framework. If you find a good use outside of unit tests for Scenario, please let us know!

Scenario is *NOT* a unit testing framework. It is intended to be used alongside a unit testing framework like xUnit or NUnit; however, it may replace common features among some of these frameworks, such as class fixtures.

## Why?

Writing tests can be really tedious and repetitive, and often difficult to maintain regularly, particularly within codebases that are in flux.

Scenario is a simple framework intended to reduce that friction as much as possible. It's got most of the boilerplate so you don't have to write it yourself.

## Extensions

Alongside Scenario are packages that are intended to further reduce the amount of boilerplate you need to write to have sustainable tests.

### Scenario.EFCore

Adds extensions for adding DbContexts to the scenario.

At the moment, this package will only use Sqlite for on-disk or in-memory db connections. Implementation-agnostic support is incoming.

### Scenario.FakeS3

Adds extensions for adding bucket stores or IAmazonS3 implementations to the scenario, including both on-disk and in-memory stores.

### Scenario.Moq

Adds extensions for adding mocked resources using Moq, fully compatible with non-mocked resources and dependent resources.
