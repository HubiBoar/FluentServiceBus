using Microsoft.Extensions.DependencyInjection;

namespace FluentServiceBus;

public interface IPublisherProvider<TEntity>
    where TEntity : IServiceBusEntityPub
{
    public ServiceBusPublisher<TEntity> GetSender(IBuiltServiceBus serviceBus);
}

public sealed class PublisherProvider<TEntity> : IPublisherProvider<TEntity>
    where TEntity : IServiceBusEntityPub
{
    public Func<IBuiltServiceBus, ServiceBusPublisher<TEntity>> Provider { get; set; } = null!;

    public ServiceBusPublisher<TEntity> GetSender(IBuiltServiceBus serviceBus)
    {
        return Provider(serviceBus)!;
    }
}

public static class ServiceBusSenderBuilderExtensions
{
    public static void AddPublisher<TEntity>(IServiceBusEntityBuilder<TEntity> builder, out IPublisherProvider<TEntity> provider)
        where TEntity : IServiceBusEntityPub
    {
        var privateProvider = new PublisherProvider<TEntity>();
        provider = privateProvider;

        builder.AddExtension((client, admin, entity, _) => 
        { 
            privateProvider.Provider = _ => new ServiceBusPublisher<TEntity>(entity, client);

            return Task.CompletedTask;
        });
    }

    public static void AddPublisher<TEntity>(IServiceBusEntityBuilder<TEntity> builder, IServiceCollection services, out IPublisherProvider<TEntity> provider)
        where TEntity : IServiceBusEntityPub
    {
        AddPublisher(builder, out var oldProvider);

        var privateProvider = new PublisherProvider<TEntity>();
        provider = privateProvider;

        privateProvider.Provider = serviceBus => 
        {
            var sender = oldProvider.GetSender(serviceBus);
            
            services.AddKeyedSingleton(sender.Path.Value, sender);

            return sender;
        };
    }

    public static void AddPublisher<TEntity>(IServiceBusEntityBuilder<TEntity> builder, IServiceCollection services)
        where TEntity : IServiceBusEntityPub
    {
        builder.AddExtension((client, admin, entity, _) => 
        { 
            var sender = new ServiceBusPublisher<TEntity>(entity, client);
            services.AddKeyedSingleton(sender.Path.Value, sender);

            return Task.CompletedTask;
        });
    }

    public static ITopicBuilder AddPublisher(this ITopicBuilder builder, out IPublisherProvider<Topic> provider)
    {
        AddPublisher<Topic>(builder, out provider);

        return builder;
    }

    public static ITopicBuilder AddPublisher(this ITopicBuilder builder, IServiceCollection services, out IPublisherProvider<Topic> provider)
    {
        AddPublisher<Topic>(builder, services, out provider);

        return builder;
    }

    public static ITopicBuilder AddPublisher(this ITopicBuilder builder, IServiceCollection services)
    {
        AddPublisher<Topic>(builder, services);

        return builder;
    }

    public static IQueueBuilder AddPublisher(this IQueueBuilder builder, out IPublisherProvider<Queue> provider)
    {
        AddPublisher<Queue>(builder, out provider);

        return builder;
    }

    public static IQueueBuilder AddPublisher(this IQueueBuilder builder, IServiceCollection services, out IPublisherProvider<Queue> provider)
    {
        AddPublisher<Queue>(builder, services, out provider);

        return builder;
    }

    public static IQueueBuilder AddPublisher(this IQueueBuilder builder, IServiceCollection services)
    {
        AddPublisher<Queue>(builder, services);

        return builder;
    }
}