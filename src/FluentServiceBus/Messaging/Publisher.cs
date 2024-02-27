using Azure.Messaging.ServiceBus;

namespace FluentServiceBus;

public sealed class ServiceBusPublisher<TEntity>
    where TEntity : IServiceBusEntityPub
{
    public IServiceBusEntityPub.IPath Path { get; }

    public ServiceBusSender Sender { get; }

    public ServiceBusPublisher(TEntity pub, ServiceBusClient client)
    {
        Path = pub.Path;
        Sender = client.CreateSender(pub.Path.Value);
    }
    
    public Task Publish<TMessage>(TMessage message)
        where TMessage : notnull
    {
        return ServiceBusPublishing.PublishMessage(message, Sender, _ => {});
    }

    public Task Publish<TMessage>(TMessage message, Action<ServiceBusMessage> modifyMessage)
        where TMessage : notnull
    {
        return ServiceBusPublishing.PublishMessage(message, Sender, modifyMessage);
    }
}