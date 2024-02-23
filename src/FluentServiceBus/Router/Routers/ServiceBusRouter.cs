using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace FluentServiceBus;

public sealed class ServiceBusRouter : IServiceBusRouter
{
    private static readonly TopicName DefaultRoutingTopicName = new TopicName("Router");

    public TopicName RoutingTopicName { get; }

    private readonly IServiceBusExtender<RoutingTopic>.Factory _routingTopicFactory;
    private readonly List<IServiceBusExtender<RoutingTopic>.Extension> _extensions;
    
    private ServiceBusRouter(TopicName routingTopicName, IServiceBusExtender<RoutingTopic>.Factory routingTopicFactory)
    {
        _extensions = [];
        _routingTopicFactory = routingTopicFactory;
        RoutingTopicName = routingTopicName;
    }

    public static IQueueRoute AddQueue(QueueName name)
    {
        return Create().AddQueue(name);
    }

    public static ITopicRoute AddTopic(TopicName name)
    {
        return Create().AddTopic(name);
    }

    public static IServiceBusRouter WithStore(SubscriptionName storeName)
    {
        return new ServiceBusRouter(DefaultRoutingTopicName, async (client, admin) =>
        {   
            var topic = await DefaultRoutingTopic(client, admin);

            await topic.Topic.AddSubscription(
                storeName,
                _ => {},
                new CreateRuleOptions
                {
                    Name = "AcceptAllMessages",
                    Filter = new TrueRuleFilter()
                });

            return topic;
        });
    }

    public static IServiceBusRouter Create()
    {
        return new ServiceBusRouter(DefaultRoutingTopicName, DefaultRoutingTopic);
    }

    private static async Task<RoutingTopic> DefaultRoutingTopic(ServiceBusClient client, ServiceBusAdministrationClient admin) 
    {
        var topic = await Topic.Create(client, admin, DefaultRoutingTopicName, _ => {});

        return new RoutingTopic(topic);
    }

    public void AddExtension(IServiceBusExtender<RoutingTopic>.Extension extension)
    {
        _extensions.Add(extension);
    }

    public void AddRoute<T>(IServiceBusRouter.RouteFactory<T> factory)
        where T : IPath
    {
        _extensions.Add(async (client, admin, routing) =>
        {
            var path = await factory(client, admin, routing);
            var forwardTo = path.Path;

            await routing.Topic.AddSubscription(
                new SubscriptionName($"route-{forwardTo}"),
                options =>
                {
                    options.ForwardTo = forwardTo;
                },
                new CreateRuleOptions
                {
                    Name = "Route",
                    Filter = new CorrelationRuleFilter()
                    {
                        To = forwardTo
                    }
                });
        });
    }

    public async Task<RoutingTopic> Build(ServiceBusClient client, ServiceBusAdministrationClient admin)
    {
        var routintTopic = await _routingTopicFactory(client, admin);
        foreach(var extension in _extensions)
        {
            await extension(client, admin, routintTopic);
        }

        return routintTopic;
    }
}
