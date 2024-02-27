using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace FluentServiceBus;

public sealed class ServiceBusBuilder : IServiceBusBuilder
{
    private readonly List<IServiceBusBuilder.Extension> _extensions;

    public ServiceBusBuilder()
    {
        _extensions = [];
    }

    public async Task<IBuiltServiceBus> Build(ServiceBusClient client, ServiceBusAdministrationClient admin)
    {
        List<IServiceBusEntity> entities = [];
        foreach(var extension in _extensions)
        {
            await extension(client, admin, entities.Add);
        }

        return new BuiltServiceBus(entities, client, admin);
    }

    public void AddServiceBusExtension(IServiceBusBuilder.Extension extension)
    {
        _extensions.Add(extension);
    }
}

internal sealed class BuiltServiceBus : IBuiltServiceBus
{
    public IReadOnlyCollection<IServiceBusEntity> Entities { get; }

    public ServiceBusClient Client { get; }

    public ServiceBusAdministrationClient Admin { get; }

    public BuiltServiceBus(
        IReadOnlyCollection<IServiceBusEntity> entities,
        ServiceBusClient client,
        ServiceBusAdministrationClient admin)
    {
        Entities = entities;
        Client = client;
        Admin = admin;
    }
}

internal sealed class BuiltServiceBusWithRouter : IBuiltServiceBusWithRouter
{

    public IReadOnlyCollection<IServiceBusEntity> Entities => _builtServiceBus.Entities;

    public ServiceBusClient Client => _builtServiceBus.Client;

    public ServiceBusAdministrationClient Admin => _builtServiceBus.Admin;

    public Topic.Router Router { get; }

    private readonly IBuiltServiceBus _builtServiceBus;

    public BuiltServiceBusWithRouter(Topic.Router router, IBuiltServiceBus builtServiceBus)
    {
        Router = router;
        _builtServiceBus = builtServiceBus;
    }

}