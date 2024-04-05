using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Definit.Results;
using FluentServiceBus;
using Sample;

var connectionString = "";

var sender = await Build(connectionString);

await sender.Publish(new { Message = "TestMessage" }, "test-topic");

static async Task<IRouterPublisher> Build(string connectionString)
{
    var client = new ServiceBusClient(connectionString);
    var administrationClient = new ServiceBusAdministrationClient(connectionString);

    var built = await new ServiceBusBuilder()
        .AddQueue(new QueueName("test-queue"))
            .AddPublisher(out var testQueueSender)
            .WithConsumer<Message>(message => Result.Or<Abandon>.Success)
        .AddTopic(new TopicName("test-topic"))
            .AddPublisher(out var testTopicSender)
            .AddSubscription(new SubscriptionName("test-subscription"))
            .AddSubscription(new SubscriptionName("test-subscription-2"))
                .WithConsumer<Message>(message => Result.Or<Abandon>.Success)
        .AddQueue(new QueueName("test-qeueue-2"))
        .BuildRouterWithStore(client, administrationClient);

    await testQueueSender.GetSender(built).Publish(new { Message = "TestQueueMessage" });
    await testTopicSender.GetSender(built).Publish(new { Message = "TestTopicMessage" });

    return built.Router;
}

namespace Sample
{
    public sealed record Message();
}