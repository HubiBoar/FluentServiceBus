using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace FluentServiceBus;

public interface IQueueBuilder : IServiceBusEntityBuilder<Queue>, IServiceBusBuilder
{
    public QueueName QueueName { get; }
}

internal sealed class QueueBuilder : IQueueBuilder
{
    public QueueName QueueName { get; }
    private readonly IServiceBusBuilder _builder;
    private readonly IServiceBusEntityBuilder<Queue>.Factory _factory;
    private readonly List<IServiceBusEntityBuilder<Queue>.Extension> _extensions;

    public QueueBuilder(IServiceBusBuilder builder, QueueName queueName, IServiceBusEntityBuilder<Queue>.Factory factory)
    {
        QueueName = queueName;

        _builder = builder;
        _factory = factory;
        _extensions = [];

        builder.AddServiceBusExtension(BuildEntity);
    }

    public void AddExtension(IServiceBusEntityBuilder<Queue>.Extension extension)
    {
        _extensions.Add(extension);
    }

    private async Task BuildEntity(ServiceBusClient client, ServiceBusAdministrationClient admin, Action<IServiceBusEntity> onEntityAdded)
    {
        var queue = await _factory(client, admin);

        foreach(var extension in _extensions)
        {
            await extension(client, admin, queue, onEntityAdded);
        }

        onEntityAdded(queue);
    }

    public void AddServiceBusExtension(IServiceBusBuilder.Extension extension) => _builder.AddServiceBusExtension(extension);

    public Task<IBuiltServiceBus> Build(ServiceBusClient client, ServiceBusAdministrationClient admin) => _builder.Build(client, admin);
}
