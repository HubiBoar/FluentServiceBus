using System.Text;
using Azure.Messaging.ServiceBus;                   
using Newtonsoft.Json;
using Shared.Messaging.Consumer;

namespace Shared.Messaging.Helpers;

internal static class ServiceBusMessageHelper
{
    public static ServiceBusMessage ConvertToMessage<T>(T message)
        where T : class, IMessage
    {
        var label = T.Label;
        var jsonMessage = JsonConvert.SerializeObject(message);
        var encodedMessage = Encoding.UTF8.GetBytes(jsonMessage);

        var serviceBusMessage = new ServiceBusMessage(encodedMessage)
        {
            Subject = label.Label,
            ContentType = "application/json"
        };

        return serviceBusMessage;
    }
}