using Azure.Messaging.ServiceBus;

namespace FluentServiceBus;

public static class ServiceBusBuilderConsumerExtensions
{
    public static void WithConsumer<TEntity, TMessage>(IServiceBusEntityBuilder<TEntity> builder, ServiceBusConsumer<TMessage> consumer, Action<ServiceBusProcessorOptions> modifyProcessor)
        where TEntity : IServiceBusEntitySub
        where TMessage : notnull
    {
        builder.AddExtension((_, _, entity, _) => entity.RegisterConsumer(consumer, modifyProcessor));
    }

    public static ITopicBuilder WithConsumer<TMessage>(this ITopicSubscriptionBuilder builder, ServiceBusConsumer<TMessage> consumer)
        where TMessage : notnull
    {
        WithConsumer<Topic.Subscription, TMessage>(builder, consumer, _ => {});
        return builder;
    }

    public static ITopicBuilder WithConsumer<TMessage>(this ITopicSubscriptionBuilder builder, ServiceBusConsumer<TMessage> consumer, Action<ServiceBusProcessorOptions> modifyProcessor)
        where TMessage : notnull
    {
        WithConsumer<Topic.Subscription, TMessage>(builder, consumer, modifyProcessor);
        return builder;
    }

    public static IServiceBusBuilder WithConsumer<TMessage>(this IQueueBuilder builder, ServiceBusConsumer<TMessage> consumer)
        where TMessage : notnull
    {
        WithConsumer<Queue, TMessage>(builder, consumer, _ => {});

        return builder;
    }

    public static IServiceBusBuilder WithConsumer<TMessage>(this IQueueBuilder builder, ServiceBusConsumer<TMessage> consumer, Action<ServiceBusProcessorOptions> modifyProcessor)
        where TMessage : notnull
    {
        WithConsumer<Queue, TMessage>(builder, consumer, modifyProcessor);

        return builder;
    }
}