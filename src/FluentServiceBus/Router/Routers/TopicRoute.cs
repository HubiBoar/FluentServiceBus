using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace FluentServiceBus;

public interface ITopicRoute : IServiceBusExtender<Topic>, IServiceBusRouter
{
}

public sealed class TopicRoute : ITopicRoute
{
    public TopicName RoutingTopicName => _router.RoutingTopicName;

    private readonly IServiceBusRouter _router;
    private readonly List<IServiceBusExtender<Topic>.Extension> _extensions;

    public TopicRoute(IServiceBusRouter router, IServiceBusExtender<Topic>.Factory factory)
    {
        _extensions = [];
        _router = router;

        router.AddRoute(async (client, admin, routing) => {
            var topic = await factory(client, admin);
            foreach(var extension in _extensions)
            {
                await extension(client, admin, topic);
            }

            return topic;
        });
    }
    
    public void AddExtension(IServiceBusExtender<Topic>.Extension extension) 
    {
        _extensions.Add(extension);
    }

    public void AddExtension(IServiceBusExtender<RoutingTopic>.Extension extension) => _router.AddExtension(extension);

    public void AddRoute<T>(IServiceBusRouter.RouteFactory<T> factory) where T : IPath => _router.AddRoute(factory);

    public Task<RoutingTopic> Build(ServiceBusClient client, ServiceBusAdministrationClient admin) => _router.Build(client, admin);
}