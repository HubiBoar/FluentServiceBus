using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace FluentServiceBus;

public interface ITopicSubscriptionBuilder : ITopicBuilder, IServiceBusEntityBuilder<Topic.Subscription>
{
    public SubscriptionName SubscriptionName { get; }
}

internal sealed class TopicSubscriptionBuilder : ITopicSubscriptionBuilder
{
    public delegate Task<Topic.Subscription> Factory(ServiceBusClient client, ServiceBusAdministrationClient admin, Topic topic);
    public TopicName TopicName => _builder.TopicName;
    public SubscriptionName SubscriptionName { get; }

    private readonly ITopicBuilder _builder;
    private readonly List<IServiceBusEntityBuilder<Topic.Subscription>.Extension> _extensions;
    private readonly Factory _factory;


    public TopicSubscriptionBuilder(ITopicBuilder builder, SubscriptionName subscriptionName, Factory factory)
    {
        _builder = builder;
        SubscriptionName = subscriptionName;
        _extensions = [];
        _factory = factory;
        builder.AddExtension(BuildEntity);
    }

    public void AddExtension(IServiceBusEntityBuilder<Topic.Subscription>.Extension extension)
    {
        _extensions.Add(extension);
    }

    private async Task BuildEntity(ServiceBusClient client, ServiceBusAdministrationClient admin, Topic topic, Action<IServiceBusEntity> onEntityAdded)
    {
        var subscription = await _factory(client, admin, topic);
        foreach(var extension in _extensions)
        {
            await extension(client, admin, subscription, onEntityAdded);
        }

        onEntityAdded(subscription);
    }

    public void AddExtension(IServiceBusEntityBuilder<Topic>.Extension extension) => _builder.AddExtension(extension);

    public void AddServiceBusExtension(IServiceBusBuilder.Extension extension) => _builder.AddServiceBusExtension(extension);

    public Task<IBuiltServiceBus> Build(ServiceBusClient client, ServiceBusAdministrationClient admin) => _builder.Build(client, admin);

}