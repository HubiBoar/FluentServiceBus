using Azure.Messaging.ServiceBus;

namespace FluentServiceBus;

public interface IServiceBusEntity
{
}

public interface IServiceBusEntityPub : IServiceBusEntity
{
    public interface IPath
    {
        public string Value { get; }
    }

    public IPath Path { get; }

    public Task PublishMessage<TMessage>(TMessage message)
        where TMessage : notnull;
}

public interface IServiceBusEntitySub : IServiceBusEntity
{
    public Task RegisterConsumer<TMessage>(ServiceBusConsumer<TMessage> consumer, Action<ServiceBusProcessorOptions> modifyProcessor)
        where TMessage : notnull;
}