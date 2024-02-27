using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace FluentServiceBus;

public interface IMessage
{
    public abstract static string Path { get; }
}

public interface IRouterPublisher
{
    Task Publish<T>(T message)
        where T : class, IMessage;

    Task Publish<T>(T message, string path)
        where T : notnull;
}

public sealed partial class Topic
{
    public sealed class Router : IServiceBusEntity, IRouterPublisher
    {
        public Topic Topic { get; }

        public Router(Topic topic)
        {
            Topic = topic;
        }

        public Task Publish<T>(T message)
            where T : class, IMessage
        {
            return Publish(message, T.Path);
        }

        public Task Publish<T>(T message, string path)
            where T : notnull
        {
            return ServiceBusPublishing.PublishMessage(message, Topic.Client, Topic.Path, serviceBusMessage => serviceBusMessage.To = path);
        }

        public Task<Subscription> AddRoute(IServiceBusEntityPub.IPath path)
        {
            var subscriptionName = new SubscriptionName($"route--{path.Value}");
            var forwardTo = path.Value;

            return Topic.AddSubscription(
                subscriptionName,
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
        }

        private static readonly TopicName DefaultRoutingTopicName = new TopicName("Router");

        public static async Task<Router> Create(ServiceBusClient client, ServiceBusAdministrationClient admin) 
        {
            var topic = await Topic.Create(client, admin, DefaultRoutingTopicName, _ => {});

            return new Router(topic);
        }
        
        public static async Task<Router> CreateWithStore(SubscriptionName storeName, ServiceBusClient client, ServiceBusAdministrationClient admin) 
        {
            var topic = await Topic.Create(client, admin, DefaultRoutingTopicName, _ => {});

            await topic.AddSubscription(
                storeName,
                _ => {},
                new CreateRuleOptions
                {
                    Name = "AcceptAllMessages",
                    Filter = new TrueRuleFilter()
                });

            return new Router(topic);
        }

        public static Task<Router> CreateWithStore(ServiceBusClient client, ServiceBusAdministrationClient admin) 
        {
            return CreateWithStore(new SubscriptionName("Store"), client, admin);
        }
    }
}