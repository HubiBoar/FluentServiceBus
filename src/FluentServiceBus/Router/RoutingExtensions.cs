using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.DependencyInjection;

namespace FluentServiceBus;

public static class RoutingExtensions
{
    public static ITopicRoute AddSubscription(this ITopicRoute topicRoute, SubscriptionName name)
    {
        return new TopicSubscriptionRoute(
            topicRoute, 
            (client, admin, topic) => 
            {
                var path = topic.Name.Value;
                var subscriptionName = name.Value;

                return topic.AddSubscription(
                    name,
                    _ => {},
                    new CreateRuleOptions
                    {
                        Name = "Route",
                        Filter = new SqlRuleFilter($"((sys.to = '{path}') AND (NOT EXISTS (user.SubscriptionName))) OR ((sys.to = '{path}') AND (user.SubscriptionName = '{subscriptionName}'))"),
                        Action = new SqlRuleAction($"SET user.SubscriptionName = '{subscriptionName}'")
                    });
            });
    }

    public static IQueueRoute AddQueue(this IServiceBusRouter router, QueueName name)
    {
        return new QueueRoute(
            router,
            (client, admin) => Queue.Create(client, admin, name, _ => {})
        );
    }

    public static ITopicRoute AddTopic(this IServiceBusRouter router, TopicName name)
    {
        return new TopicRoute(
            router,
            (client, admin) => Topic.Create(client, admin, name, _ => {})
        );
    }

    public static async Task Build(
        this IServiceBusRouter router,
        IServiceCollection services,
        ServiceBusClient client,
        ServiceBusAdministrationClient admin)
    {
        var routingTopic = await router.Build(client, admin);

        services.AddSingleton<IPublisher>(routingTopic);
    }
}
