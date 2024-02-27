using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace FluentServiceBus;

public sealed record QueueName(string Value) : IServiceBusEntityPub.IPath;

public sealed partial class Queue : IServiceBusEntityPub, IServiceBusEntitySub
{
    public IServiceBusEntityPub.IPath Path => Name;

    public QueueName Name { get; }
    public QueueProperties Properties { get; }

    private readonly ServiceBusClient _client;

    private Queue(
        ServiceBusClient client,
        QueueProperties properties)
    {
        Properties = properties;
        Name = new (Properties.Name);

        _client = client;
    }

    public static async Task<Queue> Create(
        ServiceBusClient client,
        ServiceBusAdministrationClient administrationClient,
        QueueName name,
        Action<CreateQueueOptions> setup)
    {
        var ququeName = name.Value;
        if (await administrationClient.QueueExistsAsync(ququeName))
        {
            var properties = await administrationClient.GetQueueAsync(ququeName);
            return new Queue(client, properties);
        }
        else
        {
            var options = new CreateQueueOptions(ququeName);
            setup(options);
            var properties = await administrationClient.CreateQueueAsync(options);

            return new Queue(client, properties);
        }
    }

    public Task PublishMessage<TMessage>(TMessage message)
        where TMessage : notnull
    {
        return ServiceBusPublishing.PublishMessage(message, _client, Path, _ => {});
    }

    public Task RegisterConsumer<TMessage>(ServiceBusConsumer<TMessage> consumer, Action<ServiceBusProcessorOptions> modifyProcessor)
        where TMessage : notnull
    {
        return ServiceBusProcessing.RegisterConsumer(consumer, _client, (client, options) => client.CreateProcessor(Name.Value, options), modifyProcessor);
    }
}