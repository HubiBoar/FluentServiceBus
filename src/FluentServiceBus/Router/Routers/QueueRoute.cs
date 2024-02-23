using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace FluentServiceBus;

public interface IQueueRoute : IServiceBusExtender<Queue>, IServiceBusRouter
{
}


public sealed class QueueRoute : IQueueRoute
{
    public TopicName RoutingTopicName => _router.RoutingTopicName;

    private readonly IServiceBusRouter _router;
    private readonly List<IServiceBusExtender<Queue>.Extension> _extensions;

    public QueueRoute(IServiceBusRouter router, IServiceBusExtender<Queue>.Factory factory)
    {
        _router = router;
        _extensions = [];
        router.AddRoute(async (client, admin, routing) => {
            var queue = await factory(client, admin);
            foreach(var extension in _extensions)
            {
                await extension(client, admin, queue);
            }

            return queue;
        });
    }

    public void AddExtension(IServiceBusExtender<Queue>.Extension extension) 
    {
        _extensions.Add(extension);
    }

    public void AddExtension(IServiceBusExtender<RoutingTopic>.Extension extension) => _router.AddExtension(extension);

    public void AddRoute<T>(IServiceBusRouter.RouteFactory<T> factory) where T : IPath => _router.AddRoute(factory);

    public Task<RoutingTopic> Build(ServiceBusClient client, ServiceBusAdministrationClient admin) => _router.Build(client, admin);
}