using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace FluentServiceBus;

public sealed record QueueName(string Value);

public sealed partial class Queue : IEntity
{
    public string Path => Name.Value;

    public QueueName Name { get; }
    public QueueProperties Properties { get; }

    public ServiceBusClient Client { get; }
    public ServiceBusAdministrationClient AdministrationClient { get; }
    public ServiceBusSender Sender { get; }

    private Queue(
        ServiceBusClient client,
        ServiceBusAdministrationClient administrationClient,
        QueueName name,
        QueueProperties properties)
    {
        Client = client;
        AdministrationClient = administrationClient;
        Name = name;
        Properties = properties;
        Sender = Client.CreateSender(Path);
    }

    public static async Task<Queue> Create(
        ServiceBusClient client,
        ServiceBusAdministrationClient administrationClient,
        QueueName name,
        Action<CreateQueueOptions> setup)
    {
        var ququeName = name.Value;
        if (await administrationClient.TopicExistsAsync(ququeName))
        {
            var properties = await administrationClient.GetQueueAsync(ququeName);
            return new Queue(client, administrationClient, name, properties); 
        }
        else
        {
            var options = new CreateQueueOptions(ququeName);
            setup(options);
            var properties = await administrationClient.CreateQueueAsync(options);

            return new Queue(client, administrationClient, name, properties);
        }
    }

}