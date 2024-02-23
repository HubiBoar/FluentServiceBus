using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using FluentServiceBus;
using Microsoft.Extensions.DependencyInjection;

var connectionString = "";

var sender = await Build(connectionString);

await sender.Send(new { Message = "TestMessage" }, "test-topic");

static Task<RoutingTopic> Build(string connectionString)
{
    var client = new ServiceBusClient(connectionString);
    var administrationClient = new ServiceBusAdministrationClient(connectionString);

    return ServiceBusRouter
        .AddQueue(new QueueName("test-queue"))
        .AddTopic(new TopicName("test-topic"))
            .AddSubscription(new SubscriptionName("test-subscription"))
            .AddSubscription(new SubscriptionName("test-subscription-2"))
        .AddQueue(new QueueName("test-qeueue-2"))
        .Build(client, administrationClient);
}

static void BuildInBackground(IServiceCollection services, string connectionString)
{
    var client = new ServiceBusClient(connectionString);
    var administrationClient = new ServiceBusAdministrationClient(connectionString);

    ServiceBusRouter
        .AddQueue(new QueueName("test-queue"))
        .AddTopic(new TopicName("test-topic"))
            .AddSubscription(new SubscriptionName("test-subscription"))
            .AddSubscription(new SubscriptionName("test-subscription-2"))
        .AddQueue(new QueueName("test-qeueue-2"))
        .BuildInTheBackground(services, client, administrationClient);
}