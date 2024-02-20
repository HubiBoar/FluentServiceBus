using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Logging;
using Shared.Messaging.Consumer;
using Shared.Messaging.Helpers;
using Shared.Utils;

namespace Shared.Messaging;

internal sealed class ServiceBusConsumerConfiguration : IMessagingConsumerConfiguration
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusAdministrationClient _managementClient;
    private readonly ILoggerFactory _loggerFactory;

    public ServiceBusConsumerConfiguration(
        ServiceBusClient client,
        ServiceBusAdministrationClient managementClient,
        ILoggerFactory loggerFactory)
    {
        _client = client;
        _managementClient = managementClient;
        _loggerFactory = loggerFactory;
    }

    public async Task SendMessage<T>(T message, IMessageConsumer<T> consumer)
        where T : class, IMessage
    {
        var consumerName = consumer.ConsumerName;
        var consumerPath = ConsumerPath.GetConsumerPath(consumerName);
        await CreateQueue(consumerPath);

        var serviceBusMessage = ServiceBusMessageHelper.ConvertToMessage(message);
        await _client.CreateSender(consumerPath.Path).SendMessageAsync(serviceBusMessage);
    }

    public async Task RegisterConsumer<T>(IMessageConsumer<T> consumer)
        where T : class, IMessage
    {
        var consumerName = consumer.ConsumerName;
        var consumerPath = ConsumerPath.GetConsumerPath(consumerName);
        await CreateQueue(consumerPath);

        var processorOptions = ServiceBusProcessorHelper.DefaultProcessorOptions();
        var processor = _client.CreateProcessor(consumerPath.Path, processorOptions);
        
        processor.ProcessMessageAsync += client => ProcessMessage(client, consumer, consumerName);
        processor.ProcessErrorAsync += error => ProcessError(error, consumerName);

        await processor.StartProcessingAsync();
    }

    private async Task CreateQueue(ConsumerPath consumerPath)
    {
        if (await _managementClient.QueueExistsAsync(consumerPath.Path) == false)
        {
            await _managementClient.CreateQueueAsync(new CreateQueueOptions(consumerPath.Path)
            {
                DeadLetteringOnMessageExpiration = true
            });
        }
    }

    private Task ProcessMessage<T>(
        ProcessMessageEventArgs client,
        IMessageConsumer<T> consumer,
        ConsumerName consumerName)
        where T : class, IMessage
    {
        var serviceBusMessage = client.Message;
        var consumerLogger = IConsumerLogger.CreateConsumerLogger(
            _loggerFactory,
            serviceBusMessage.MessageId,
            consumerName);

        try
        {
            consumerLogger.LogInformation("Received message");

            return ServiceBusReceivedMessageHelper.TryDeserialize<T>(serviceBusMessage).Match<Task>(
                async deserializedMessage =>
                {
                    var result = await consumer.OnMessageInternal(deserializedMessage, consumerLogger);

                    await result.Match<Task>(
                        success => CompleteMessage(client, serviceBusMessage, consumerLogger),
                        error => DeadLetterMessageHelper.Message(client, consumerLogger, error));
                },
                deserializationError => DeadLetterMessageHelper.Message(client, consumerLogger, deserializationError));
        }
        catch (Exception exception)
        {
            return DeadLetterMessageHelper.Message(client, consumerLogger, new Error(exception));
        }
    }
    
    private Task ProcessError(
        ProcessErrorEventArgs error,
        ConsumerName consumerName)
    {
        var consumerLogger = IConsumerLogger.CreateConsumerLogger(
            _loggerFactory,
            "ProcessErrorAsync Unknown Message",
            consumerName);

        consumerLogger.LogError(error.Exception, "ProcessErrorAsync Error");

        return Task.CompletedTask;
    }

    private async Task CompleteMessage(
        ProcessMessageEventArgs client,
        ServiceBusReceivedMessage message,
        IConsumerLogger logger)
    {
        await client.CompleteMessageAsync(message);
        logger.Log(LogLevel.Information, "Completed Message Successfully");
    }
}