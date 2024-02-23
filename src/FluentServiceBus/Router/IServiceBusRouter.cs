using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace FluentServiceBus;

public interface IServiceBusRouter : IServiceBusExtender<RoutingTopic>
{
    public delegate Task<T> RouteFactory<T>(ServiceBusClient client, ServiceBusAdministrationClient admin, RoutingTopic routingTopic)
        where T : IPath;

    public TopicName RoutingTopicName { get; }

    public void AddRoute<T>(RouteFactory<T> factory)
        where T : IPath;
    public Task<RoutingTopic> Build(ServiceBusClient client, ServiceBusAdministrationClient admin);
}

public interface IServiceBusExtender<TPath>
    where TPath : IPath
{
    public delegate Task Extension(ServiceBusClient client, ServiceBusAdministrationClient admin, TPath path);
    public delegate Task<TPath> Factory(ServiceBusClient client, ServiceBusAdministrationClient admin);

    public void AddExtension(Extension extension);
}