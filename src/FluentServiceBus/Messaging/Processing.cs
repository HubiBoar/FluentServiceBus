using Azure.Messaging.ServiceBus;
using System.Text;
using Newtonsoft.Json;
using Definit.Results;

namespace FluentServiceBus;

internal sealed class ServiceBusProcessing
{
    public delegate ServiceBusProcessor ProcessorFactory(ServiceBusClient client, ServiceBusProcessorOptions options);

    public static async Task RegisterConsumer<TMessage>(
        ServiceBusConsumer<TMessage> consumer,
        ServiceBusClient client,
        ProcessorFactory processorFactory,
        Action<ServiceBusProcessorOptions> modifyProcessor)
        where TMessage : notnull
    {
        var processorOptions = DefaultProcessorOptions();
        modifyProcessor(processorOptions);
        var processor = processorFactory(client, processorOptions);

        processor.ProcessMessageAsync += args => ProcessMessage(args, consumer);
        processor.ProcessErrorAsync += ProcessError;

        await processor.StartProcessingAsync();
    }

    private static ServiceBusProcessorOptions DefaultProcessorOptions()
    {
        return new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 1,
        };
    }

    private static Task ProcessMessage<TMessage>(
        ProcessMessageEventArgs args,
        ServiceBusConsumer<TMessage> consumer)
        where TMessage : notnull
    {
        var serviceBusMessage = args.Message;

        try
        {
            return TryDeserialize<TMessage>(serviceBusMessage).Match(
                async deserializedMessage =>
                {
                    var result = await consumer(deserializedMessage);

                    await result.Match(
                        success => CompleteMessage(args, serviceBusMessage),
                        abandon => AbandonMessage(args, serviceBusMessage),
                        error => DeadLetterMessageHelper.Message(args, error));
                },
                deserializationError => DeadLetterMessageHelper.Message(args, deserializationError));
        }
        catch (Exception exception)
        {
            return DeadLetterMessageHelper.Message(args, new Error(exception.Message));
        }
    }
    
    private static Task ProcessError(
        ProcessErrorEventArgs args)
    {
        return Task.CompletedTask;
    }

    private static async Task CompleteMessage(
        ProcessMessageEventArgs args,
        ServiceBusReceivedMessage message)
    {
        await args.CompleteMessageAsync(message);
    }
    
    private static async Task AbandonMessage(
        ProcessMessageEventArgs args,
        ServiceBusReceivedMessage message)
    {
        await args.AbandonMessageAsync(message);
    }

    private static Result<TMessage> TryDeserialize<TMessage>(ServiceBusReceivedMessage serviceBusMessage)
        where TMessage : notnull
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
            return new Error($"Encountered exception when trying to deserialize message :: {exception.Message}");
        }

        return message;
    }
}
