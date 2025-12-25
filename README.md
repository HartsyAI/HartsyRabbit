# HartsyRabbit

Shared RabbitMQ message bus library used across Hartsy ecosystem projects. 
This is an internal library that links all projects and keeps them typesafe. 

## What this repository contains

- Type-safe message bus abstractions and implementation
- Shared message contracts and message types
- RabbitMQ connection + topology setup
- DI extension methods
- Logging abstraction via `IMessageBusLogger`

## What this repository does not contain

- Any site-specific handlers
- Any site-specific logging implementation
- Any site-specific configuration binding logic beyond `MessageBusConfiguration`

## Basic usage (consumer projects)

1. Reference the project:

```xml
<ItemGroup>
  <ProjectReference Include="RabbitMQ/HartsyRabbit/HartsyRabbit.csproj" />
</ItemGroup>
```

2. Implement `IMessageBusLogger` in your site.

3. Register the bus and your handlers:

```csharp
services.AddSingleton<IMessageBusLogger, YourSiteMessageBusLogger>();
services.AddTypeSafeMessageBus(configuration);
services.AddMessageHandler<SomeMessage, SomeHandler>();
```

4. Start your hosted service that calls `ITypeSafeMessageBus.StartAsync()`.
