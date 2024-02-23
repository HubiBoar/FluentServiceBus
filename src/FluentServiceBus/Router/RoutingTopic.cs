using System.Text;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Newtonsoft.Json;

namespace FluentServiceBus;

public sealed class RoutingTopic : IPublisher, IEntity
{
    public string Path => Topic.Path;
    public ServiceBusSender Sender => Topic.Sender;
    public ServiceBusClient Client => Topic.Client;
    public ServiceBusAdministrationClient AdministrationClient => Topic.AdministrationClient;

    public Topic Topic { get; }

    public RoutingTopic(Topic topic)
    {
        Topic = topic;
    }
 
    public Task Publish<T>(T message)
        where T : IMessage
    {
        return Publish(message, T.Path);
    }

    public Task Publish(object message, string path)
    {
        var serviceBusMessage = ConvertToMessage(message, path);

        return Sender.SendMessageAsync(serviceBusMessage);
    }

    private static ServiceBusMessage ConvertToMessage(object message, string path)
    {
        var jsonMessage = JsonConvert.SerializeObject(message);
        var encodedMessage = Encoding.UTF8.GetBytes(jsonMessage);

        var serviceBusMessage = new ServiceBusMessage(encodedMessage)
        {
            To = path,
            ContentType = "application/json"
        };

        return serviceBusMessage;
    }
}