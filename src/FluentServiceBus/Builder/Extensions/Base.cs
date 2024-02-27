namespace FluentServiceBus;

public static class ServiceBusBuilderExtensions
{
    public static IQueueBuilder AddQueue(this IServiceBusBuilder builder, QueueName name)
    {
        return new QueueBuilder(builder, name, (client, admin) => Queue.Create(client, admin, name, _ => {}));
    }

    public static ITopicBuilder AddTopic(this IServiceBusBuilder builder, TopicName name)
    {
        return new TopicBuilder(builder, name, (client, admin) => Topic.Create(client, admin, name, _ => {}));
    }

    public static ITopicSubscriptionBuilder AddSubscription(this ITopicBuilder builder, SubscriptionName name)
    {
        return new TopicSubscriptionBuilder(
            builder,
            name,
            (client, admin, topic) => 
            {
                var path = topic.Name.Value;
                var subscriptionName = name.Value;

                return topic.AddRoutingSubscription(name);
            });
    }
}