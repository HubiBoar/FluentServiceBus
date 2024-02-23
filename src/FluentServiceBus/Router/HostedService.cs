using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Hosting;

namespace FluentServiceBus;

public sealed class ServiceBusRouterHostedService : BackgroundService
{
    public IServiceBusRouter router { get; }

    private readonly ServiceBusClient _client;
    private readonly ServiceBusAdministrationClient _administrationClient;

    public ServiceBusRouterHostedService(
        IServiceBusRouter builder,
        ServiceBusClient client,
        ServiceBusAdministrationClient administrationClient)
    {
        router = builder;
        _client = client;
        _administrationClient = administrationClient;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return router.Build(_client, _administrationClient);
    }
}