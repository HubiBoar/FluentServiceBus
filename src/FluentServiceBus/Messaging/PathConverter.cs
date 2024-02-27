using System.Text;

namespace FluentServiceBus;

public static class PathConverter
{
    public static TopicName ToTopicName(string name)
    {
        return new TopicName(Convert(name));
    }

    public static QueueName ToQueueName(string name)
    {
        return new QueueName(Convert(name));
    }

    public static SubscriptionName ToSubscriptionName(string name)
    {
        return new SubscriptionName(Convert(name));
    }

    public static string Convert(string name)
    {
        //Add '-' between UpperCases and set everything to lower : TestTopic -> test-topic
        var nameBuildingResult = new StringBuilder();

        foreach (var ch in name)
        {
            if (char.IsUpper(ch) && nameBuildingResult.Length > 0)
            {
                nameBuildingResult.Append('-');
            }

            nameBuildingResult.Append(ch);
        }

        return nameBuildingResult.ToString().ToLower();
    }
}