using Azure.Messaging.ServiceBus;

namespace Shared.Messaging.Helpers;

internal static class ServiceBusProcessorHelper
{
    public static ServiceBusProcessorOptions DefaultProcessorOptions()
    {
        return new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 1,
        };
    }
}