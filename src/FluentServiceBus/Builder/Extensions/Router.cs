using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.DependencyInjection;

namespace FluentServiceBus;

public static class ServiceBusBuilderRouterExtensions
{   
    public static async Task<IBuiltServiceBusWithRouter> BuildRouter(this IServiceBusBuilder builder, ServiceBusClient client, ServiceBusAdministrationClient admin, IServiceBusEntityBuilder<Topic.Router>.Factory routerFactory)
    {
        var built = await builder.Build(client, admin);

        var topic = await routerFactory(client, admin);
        foreach(var entity in built.Entities)
        {
            if(entity is IServiceBusEntityPub pub)
            {
                await topic.AddRoute(pub.Path);
            }
        }

        return new BuiltServiceBusWithRouter(topic, built);
    }

    public static async Task<IBuiltServiceBusWithRouter> BuildRouter(this IServiceBusBuilder builder, IServiceCollection services, ServiceBusClient client, ServiceBusAdministrationClient admin)
    {
        var router = await BuildRouter(builder,  client, admin, Topic.Router.Create);
        services.AddSingleton<IRouterPublisher>(router.Router);

        return router;
    }

    public static Task<IBuiltServiceBusWithRouter> BuildRouterWithStore(this IServiceBusBuilder builder, ServiceBusClient client, ServiceBusAdministrationClient admin)
    {
        return BuildRouter(builder,  client, admin, Topic.Router.CreateWithStore);
    }

    public static async Task<IBuiltServiceBusWithRouter> BuildRouterWithStore(this IServiceBusBuilder builder, IServiceCollection services, ServiceBusClient client, ServiceBusAdministrationClient admin)
    {
        var router = await BuildRouter(builder,  client, admin, Topic.Router.CreateWithStore);
        services.AddSingleton<IRouterPublisher>(router.Router);

        return router;
    }

    public static Task<IBuiltServiceBusWithRouter> BuildRouter(this IServiceBusBuilder builder, ServiceBusClient client, ServiceBusAdministrationClient admin)
    {
        return BuildRouter(builder, client, admin, Topic.Router.Create);
    }
}