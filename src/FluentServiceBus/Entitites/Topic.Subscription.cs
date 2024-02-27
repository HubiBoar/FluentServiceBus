using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace FluentServiceBus;

public sealed record SubscriptionName(string Value);

public sealed partial class Topic
{
    public IReadOnlyCollection<Subscription> Subscriptions => _subscriptions;

    private readonly List<Subscription> _subscriptions = [];

    public Task<Subscription> AddSubscription(
        SubscriptionName name,
        Action<CreateSubscriptionOptions> setup,
        params CreateRuleOptions[] rules)
    {
        return Subscription.Create(this, name, setup, rules);
    }

    public Task<Subscription> AddSubscription(SubscriptionName name, string path)
    {
        var subscriptionName = name.Value;

        return AddSubscription(
            name,
            _ => {},
            new CreateRuleOptions
            {
                Name = "Route",
                Filter = new SqlRuleFilter($"((sys.to = '{path}') AND (NOT EXISTS (user.SubscriptionName))) OR ((sys.to = '{path}') AND (user.SubscriptionName = '{subscriptionName}'))"),
                Action = new SqlRuleAction($"SET user.SubscriptionName = '{subscriptionName}'")
            });
    }

    public Task<Subscription> AddRoutingSubscription(SubscriptionName name)
    {
        return AddSubscription(name, Path.Value);
    }

    public sealed class Subscription : IServiceBusEntitySub
    {
        public SubscriptionName Name { get; }
        public Topic ParentTopic { get; }
        public SubscriptionProperties Properties { get; }

        private Subscription(Topic topic, SubscriptionProperties properties)
        {
            ParentTopic = topic;
            Properties = properties;
            Name = new (Properties.SubscriptionName);
        }

        public static async Task<Subscription> Create(
            Topic topic,
            SubscriptionName name,
            Action<CreateSubscriptionOptions> setup,
            params CreateRuleOptions[] rules)
        {
            var topicName = topic.Name.Value;
            var subscriptionName = name.Value;
            if (await topic.AdministrationClient.SubscriptionExistsAsync(topicName, subscriptionName))
            {
                var properties = await topic.AdministrationClient.GetSubscriptionAsync(topicName, subscriptionName);
                var subscription = new Subscription(topic, properties);
                topic._subscriptions.Add(subscription);
                return subscription;
            }
            else
            {
                var options = new CreateSubscriptionOptions(topicName, subscriptionName);
                setup(options);

                var firstRule = rules.FirstOrDefault() ?? new CreateRuleOptions();

                var properties = await topic.AdministrationClient.CreateSubscriptionAsync(options, firstRule);

                foreach(var rule in rules.Skip(1))
                {
                    await topic.AdministrationClient.CreateRuleAsync(topicName, subscriptionName, rule);
                }

                var subscription = new Subscription(topic, properties);
                topic._subscriptions.Add(subscription);
                return subscription;
            }
        }

        public Task PublishMessage<TMessage>(TMessage message)
            where TMessage : notnull
        {
            return ParentTopic.PublishMessage(message);
        }

        public Task AddRule(CreateRuleOptions rule)
        {
            var topicName = ParentTopic.Name.Value;
            var subscriptionName = Name.Value;
            return ParentTopic.AdministrationClient.CreateRuleAsync(topicName, subscriptionName, rule);
        }

        public Task RegisterConsumer<TMessage>(ServiceBusConsumer<TMessage> subscriber, Action<ServiceBusProcessorOptions> modifyProcessor)
            where TMessage : notnull
        {
            return ServiceBusProcessing.RegisterConsumer(subscriber, ParentTopic.Client, (client, options) => client.CreateProcessor(ParentTopic.Name.Value, Name.Value, options), modifyProcessor);
        }
    }
}
