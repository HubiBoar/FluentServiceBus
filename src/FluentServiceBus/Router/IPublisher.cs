namespace FluentServiceBus;

public interface IMessage
{
    public abstract static string Path { get; }
}

public interface IPublisher
{
    Task Publish<T>(T message)
        where T : IMessage;

    Task Publish(object message, string path);
}