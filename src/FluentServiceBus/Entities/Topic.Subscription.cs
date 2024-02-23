using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace FluentServiceBus;

public sealed record SubscriptionName(string Value);

public sealed partial class Topic
{
    public IReadOnlyCollection<Subscription> Subscriptions => _subscriptions;

    private readonly List<Subscription> _subscriptions = [];

    public sealed class Subscription : IEntity
    {
        public string Path => ParentTopic.Path;
        public ServiceBusClient Client => ParentTopic.Client;
        public ServiceBusAdministrationClient AdministrationClient => ParentTopic.AdministrationClient;
        public ServiceBusSender Sender => ParentTopic.Sender;

        public Topic ParentTopic { get; }
        public SubscriptionName Name { get; }
        public SubscriptionProperties Properties { get; }

        private Subscription(Topic topic, SubscriptionName path, SubscriptionProperties properties)
        {
            ParentTopic = topic;
            Name = path;
            Properties = properties;
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
                var subscription = new Subscription(topic, name, properties);
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

                var subscription = new Subscription(topic, name, properties);
                topic._subscriptions.Add(subscription);
                return subscription;
            }
        }

        public Task AddRule(CreateRuleOptions rule)
        {
            var topicName = ParentTopic.Name.Value;
            var subscriptionName = this.Name.Value;
            return ParentTopic.AdministrationClient.CreateRuleAsync(topicName, subscriptionName, rule);
        }
    }

    public Task<Subscription> AddSubscription(
        SubscriptionName name,
        Action<CreateSubscriptionOptions> setup,
        params CreateRuleOptions[] rules)
    {
        return Subscription.Create(this, name, setup, rules);
    }
}