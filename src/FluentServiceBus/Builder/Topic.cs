using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace FluentServiceBus;

public interface ITopicBuilder : IServiceBusEntityBuilder<Topic>, IServiceBusBuilder
{
    public TopicName TopicName { get; }
}

internal sealed class TopicBuilder : ITopicBuilder
{
    public TopicName TopicName { get; }
    private readonly IServiceBusBuilder _builder;
    private readonly IServiceBusEntityBuilder<Topic>.Factory _factory;
    private readonly List<IServiceBusEntityBuilder<Topic>.Extension> _extensions;

    public TopicBuilder(IServiceBusBuilder builder, TopicName topicName, IServiceBusEntityBuilder<Topic>.Factory factory)
    {
        TopicName = topicName;

        _builder = builder;
        _factory = factory;
        _extensions = [];
        builder.AddServiceBusExtension(BuildEntity);
    }

    public void AddExtension(IServiceBusEntityBuilder<Topic>.Extension extension)
    {
        _extensions.Add(extension);
    }

    private async Task BuildEntity(ServiceBusClient client, ServiceBusAdministrationClient admin, Action<IServiceBusEntity> onEntityAdded)
    {
        var topic = await _factory(client, admin);

        foreach(var extension in _extensions)
        {
            await extension(client, admin, topic, onEntityAdded);
        }

        onEntityAdded(topic);
    }

    public void AddServiceBusExtension(IServiceBusBuilder.Extension extension) => _builder.AddServiceBusExtension(extension);

    public Task<IBuiltServiceBus> Build(ServiceBusClient client, ServiceBusAdministrationClient admin) => _builder.Build(client, admin);
}
