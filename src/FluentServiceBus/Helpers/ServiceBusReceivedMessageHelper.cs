using System.Text;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using OneOf;
using Shared.Messaging.Consumer;
using Shared.Utils;

namespace Shared.Messaging.Helpers;

internal static class ServiceBusReceivedMessageHelper
{
    public static Result<TMessage> TryDeserialize<TMessage>(ServiceBusReceivedMessage serviceBusMessage)
        where TMessage : class, IMessage
    {
        TMessage message;
        var body = serviceBusMessage.Body.ToArray();
        var decodedBody = Encoding.UTF8.GetString(body);
        try
        {
            var newMessage = JsonConvert.DeserializeObject<TMessage>(decodedBody);
            if (newMessage is null)
            {
                return new Error("Message deserialization returned null");
            }
            else
            {
                message = newMessage;
            }
        }
        catch (Exception exception)
        {
            return new Error("Encountered exception when trying to deserialize message", exception);
        }

        return message;
    }
}