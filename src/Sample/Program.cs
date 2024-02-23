using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using FluentServiceBus;

var connectionString = "";

var sender = await Build(connectionString);

await sender.Publish(new { Message = "TestMessage" }, "test-topic");

static async Task<IPublisher> Build(string connectionString)
{
    var client = new ServiceBusClient(connectionString);
    var administrationClient = new ServiceBusAdministrationClient(connectionString);

    return await ServiceBusRouter
        .AddQueue(new QueueName("test-queue"))
        .AddTopic(new TopicName("test-topic"))
            .AddSubscription(new SubscriptionName("test-subscription"))
            .AddSubscription(new SubscriptionName("test-subscription-2"))
        .AddQueue(new QueueName("test-qeueue-2"))
        .Build(client, administrationClient);
}