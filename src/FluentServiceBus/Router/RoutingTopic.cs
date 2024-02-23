using System.Text;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Newtonsoft.Json;

namespace FluentServiceBus;

public interface IMessage
{
    public abstract static string Path { get; }
}


public interface IRoutingTopicSender
{
    Task Send<T>(T message)
        where T : IMessage;

    Task Send(object message, string path);
}

public sealed class RoutingTopicSender : IRoutingTopicSender
{
    private readonly ServiceBusSender _sender;

    public RoutingTopicSender(ServiceBusSender sender)
    {
        _sender = sender;
    }

    public Task Send<T>(T message)
        where T : IMessage
    {
        return Send(message, T.Path);
    }

    public Task Send(object message, string path)
    {
        var serviceBusMessage = ConvertToMessage(message, path);

        return _sender.SendMessageAsync(serviceBusMessage);
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

public sealed class RoutingTopic : IRoutingTopicSender, IEntity
{
    public string Path => Topic.Path;
    public ServiceBusSender Sender => Topic.Sender;
    public ServiceBusClient Client => Topic.Client;
    public ServiceBusAdministrationClient AdministrationClient => Topic.AdministrationClient;

    public Topic Topic { get; }

    private readonly RoutingTopicSender _sender;

    public RoutingTopic(Topic topic)
    {
        Topic = topic;
        _sender = new RoutingTopicSender(Sender);
    }

    public Task Send<T>(T message) where T : IMessage => _sender.Send(message);

    public Task Send(object message, string path) => _sender.Send(message, path);
}