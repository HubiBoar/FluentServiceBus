using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace FluentServiceBus;

public sealed record TopicName(string Value);

public sealed partial class Topic : IEntity
{
    public string Path => Name.Value;

    public TopicName Name { get; }
    public TopicProperties Properties { get; }

    public ServiceBusClient Client { get; }
    public ServiceBusAdministrationClient AdministrationClient { get; }
    public ServiceBusSender Sender { get; }

    private Topic(
        ServiceBusClient client,
        ServiceBusAdministrationClient administrationClient,
        TopicName name,
        TopicProperties properties)
    {
        Client = client;
        AdministrationClient = administrationClient;
        Name = name;
        Properties = properties;
        Sender = client.CreateSender(Path);
    }

    public static async Task<Topic> Create(
        ServiceBusClient client,
        ServiceBusAdministrationClient administrationClient,
        TopicName name,
        Action<CreateTopicOptions> setup)
    {
        var topicName = name.Value;
        if (await administrationClient.TopicExistsAsync(topicName))
        {
            var properties = await administrationClient.GetTopicAsync(topicName);
            return new Topic(client, administrationClient, name, properties); 
        }
        else
        {
            var options = new CreateTopicOptions(topicName);
            setup(options);
            var properties = await administrationClient.CreateTopicAsync(options);

            return new Topic(client, administrationClient, name, properties);
        }
    }
}