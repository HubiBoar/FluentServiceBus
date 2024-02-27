using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace FluentServiceBus;

public interface IServiceBusBuilder
{
    public delegate Task Extension(ServiceBusClient client, ServiceBusAdministrationClient admin, Action<IServiceBusEntity> addEntity);

    public void AddServiceBusExtension(Extension extension);

    public Task<IBuiltServiceBus> Build(ServiceBusClient client, ServiceBusAdministrationClient admin);
}

public interface IServiceBusEntityBuilder<TEntity>
    where TEntity : IServiceBusEntity
{
    public delegate Task<TEntity> Factory(ServiceBusClient client, ServiceBusAdministrationClient admin);

    public delegate Task Extension(ServiceBusClient client, ServiceBusAdministrationClient admin, TEntity entity, Action<IServiceBusEntity> addEntity);

    public void AddExtension(Extension extension);
}

public interface IBuiltServiceBus
{
    public IReadOnlyCollection<IServiceBusEntity> Entities { get; }

    public ServiceBusClient Client { get; }
    public ServiceBusAdministrationClient Admin { get; }
}

public interface IBuiltServiceBusWithRouter : IBuiltServiceBus
{
    public Topic.Router Router { get; }
}