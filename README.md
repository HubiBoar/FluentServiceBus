# FluentServiceBus

![Release Status](https://img.shields.io/github/actions/workflow/status/HubiBoar/FluentServiceBus/publish.yml)
![NuGet Version](https://img.shields.io/nuget/v/FluentServiceBus)
![NuGet Downloads](https://img.shields.io/nuget/dt/FluentServiceBus)

FluentServiceBus is an library aiming to help with Azure ServiceBus setup using an Fluent/LINQ like API.

### Queues
```csharp
var client = new ServiceBusClient(connectionString);
var administrationClient = new ServiceBusAdministrationClient(connectionString);

await new ServiceBusBuilder()
    .AddQueue(new QueueName("test-queue"))
    .Build(client, administrationClient);
```

### Topics
```csharp
new ServiceBusBuilder()
    .AddTopic(new TopicName("test-topic"))
        .AddSubscription(new SubscriptionName("test-subscription"))
        .AddSubscription(new SubscriptionName("test-subscription-2"))
```

### Consumers
```csharp
new ServiceBusBuilder()
    .AddQueue(new QueueName("test-queue"))
        .WithConsumer<Message>(message => Result.Or<Abandon>.Success)
    .AddTopic(new TopicName("test-topic"))
        .AddSubscription(new SubscriptionName("test-subscription"))
             .WithConsumer<Message>(message => Result.Or<Abandon>.Success)
        .AddSubscription(new SubscriptionName("test-subscription-2"))
            .WithConsumer<Message>(message => Result.Or<Abandon>.Success)
```

### Publishers
```csharp
var built = await new ServiceBusBuilder()
      .AddQueue(new QueueName("test-queue"))
          .AddPublisher(out var testQueueSender)
      .AddTopic(new TopicName("test-topic"))
          .AddPublisher(out var testTopicSender)
          .AddSubscription(new SubscriptionName("test-subscription"))
      .Build(client, administrationClient);

await testQueueSender.GetSender(built).Publish(new { Message = "TestQueueMessage" });
await testTopicSender.GetSender(built).Publish(new { Message = "TestTopicMessage" });
```


### Routing

```csharp
var router = await new ServiceBusBuilder()
      .AddQueue(new QueueName("test-queue"))
          .WithConsumer<Message>(message => Result.Or<Abandon>.Success)
      .AddTopic(new TopicName("test-topic"))
          .AddSubscription(new SubscriptionName("test-subscription"))
          .AddSubscription(new SubscriptionName("test-subscription-2"))
              .WithConsumer<Message>(message => Result.Or<Abandon>.Success)
      .AddQueue(new QueueName("test-qeueue-2"))
      .BuildRouterWithStore(client, administrationClient)
      .Router;

await router.Publish(new { Message = "TestMessage" }, "test-topic");
```

## License

The code in this repo is licensed under the [MIT](LICENSE.TXT) license.
