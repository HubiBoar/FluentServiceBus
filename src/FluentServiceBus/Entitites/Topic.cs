using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace FluentServiceBus;

public sealed record TopicName(string Value) : IServiceBusEntityPub.IPath;

public sealed partial class Topic : IServiceBusEntityPub
{
    public TopicName Name { get; }
    public IServiceBusEntityPub.IPath Path => Name;
    public TopicProperties Properties { get; }

    private ServiceBusClient Client { get; }
    private ServiceBusAdministrationClient AdministrationClient { get; }

    private Topic(
        ServiceBusClient client,
        ServiceBusAdministrationClient administrationClient,
        TopicProperties properties)
    {
        Properties = properties;
        Name = new (Properties.Name);

        Client = client;
        AdministrationClient = administrationClient;
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
            return new Topic(client, administrationClient, properties); 
        }
        else
        {
            var options = new CreateTopicOptions(topicName);
            setup(options);
            var properties = await administrationClient.CreateTopicAsync(options);

            return new Topic(client, administrationClient, properties);
        }
    }

    public Task PublishMessage<TMessage>(TMessage message)
        where TMessage : notnull
    {
        return ServiceBusPublishing.PublishMessage(message, Client, Path, _ => {});
    }
}