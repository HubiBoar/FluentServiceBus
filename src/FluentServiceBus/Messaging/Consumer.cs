using Definit.Results;

namespace FluentServiceBus;

public sealed record Abandon();

public delegate Task<Result.Or<Abandon>> ServiceBusConsumer<TMessage>(TMessage message)
    where TMessage : notnull;