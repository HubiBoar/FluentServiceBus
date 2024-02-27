using Azure.Messaging.ServiceBus;
using System.Text;
using Newtonsoft.Json;

namespace FluentServiceBus;

internal sealed class ServiceBusPublishing
{
    public static Task PublishMessage<TMessage>(TMessage message, ServiceBusClient client, IServiceBusEntityPub.IPath path, Action<ServiceBusMessage> modifyMessage)
        where TMessage : notnull
    {
        var sender = client.CreateSender(path.Value);
        return PublishMessage(message, sender, modifyMessage);
    }

        public static Task PublishMessage<TMessage>(TMessage message, ServiceBusSender sender, Action<ServiceBusMessage> modifyMessage)
        where TMessage : notnull
    {
        var serviceBusMessage = ConvertToMessage(message, modifyMessage);

        return sender.SendMessageAsync(serviceBusMessage);
    }

    private static ServiceBusMessage ConvertToMessage<TMessage>(TMessage message, Action<ServiceBusMessage> modifyMessage)
        where TMessage : notnull
    {
        var jsonMessage = JsonConvert.SerializeObject(message);
        var encodedMessage = Encoding.UTF8.GetBytes(jsonMessage);

        var serviceBusMessage = new ServiceBusMessage(encodedMessage)
        {
            ContentType = "application/json"
        };

        modifyMessage(serviceBusMessage);

        return serviceBusMessage;
    }
}