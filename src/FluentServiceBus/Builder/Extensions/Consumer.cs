namespace FluentServiceBus;

public static class ServiceBusBuilderConsumerExtensions
{
    public static void WithConsumer<TEntity, TMessage>(IServiceBusEntityBuilder<TEntity> builder, ServiceBusConsumer<TMessage> subscriber)
        where TEntity : IServiceBusEntitySub
        where TMessage : notnull
    {
        builder.AddExtension((_, _, entity, _) => entity.RegisterConsumer(subscriber));
    }

    public static ITopicBuilder WithConsumer<TMessage>(this ITopicSubscriptionBuilder builder, ServiceBusConsumer<TMessage> consumer)
        where TMessage : notnull
    {
        WithConsumer<Topic.Subscription, TMessage>(builder, consumer);
        return builder;
    }

    public static IServiceBusBuilder WithConsumer<TMessage>(this IQueueBuilder builder, ServiceBusConsumer<TMessage> subscriber)
        where TMessage : notnull
    {
        WithConsumer<Queue, TMessage>(builder, subscriber);
        return builder;
    }
}