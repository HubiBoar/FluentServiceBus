using OneOf;
using OneOf.Types;

namespace FluentServiceBus;

public sealed record Abandon();

public sealed record DeadLetter(string Message);

public delegate Task<OneOf<Success, Abandon, DeadLetter>> ServiceBusConsumer<TMessage>(TMessage message)
    where TMessage : notnull;