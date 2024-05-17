# FluentServiceBus

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
        .AddSubscription(new SubscriptionName("test-subscription-2"))
            .WithConsumer<Message>(message => Result.Or<Abandon>.Success)
```

### Publishers
```csharp
var built = await new ServiceBusBuilder()
      .AddQueue(new QueueName("test-queue"))
          .AddPublisher(out var testQueueSender)
          .WithConsumer<Message>(message => Result.Or<Abandon>.Success)
      .AddTopic(new TopicName("test-topic"))
          .AddPublisher(out var testTopicSender)
          .AddSubscription(new SubscriptionName("test-subscription"))
          .AddSubscription(new SubscriptionName("test-subscription-2"))
              .WithConsumer<Message>(message => Result.Or<Abandon>.Success)
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
