using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace FluentServiceBus;

public sealed class TopicSubscriptionRoute :
    ITopicRoute,
    IServiceBusExtender<Topic.Subscription>
{
    public delegate Task<Topic.Subscription> Factory(ServiceBusClient client, ServiceBusAdministrationClient admin, Topic topic);

    public TopicName RoutingTopicName => _router.RoutingTopicName;

    private readonly ITopicRoute _router;
    private readonly List<IServiceBusExtender<Topic.Subscription>.Extension> _extensions;

    public TopicSubscriptionRoute(ITopicRoute router, Factory factory)
    {
        _extensions = [];
        _router = router;

        router.AddExtension(Extend);

        async Task Extend(ServiceBusClient client, ServiceBusAdministrationClient admin, Topic topic)
        {
            var subscription = await factory(client, admin, topic);
            foreach(var extension in _extensions)
            {
                await extension(client, admin, subscription);
            }
        }
    }

    public void AddExtension(IServiceBusExtender<Topic.Subscription>.Extension extension)
    {
        _extensions.Add(extension);
    }

    public void AddExtension(IServiceBusExtender<Topic>.Extension extension) => _router.AddExtension(extension);

    public void AddRoute<T>(IServiceBusRouter.RouteFactory<T> factory) where T : IPath => _router.AddRoute(factory);

    public void AddExtension(IServiceBusExtender<RoutingTopic>.Extension extension) => _router.AddExtension(extension);

    public Task<RoutingTopic> Build(ServiceBusClient client, ServiceBusAdministrationClient admin) => _router.Build(client, admin);
}
