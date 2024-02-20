using System.Text;
using Shared.Messaging.Consumer;

namespace Shared.Messaging.Helpers;

internal sealed class ConsumerPath
{
    public string Path { get; }

    private ConsumerPath(ConsumerName consumerName)
    {
        var lowerConsumerName = SplitOnUpperCase(consumerName).ToLower();
        Path = lowerConsumerName;
    }

    public static ConsumerPath GetConsumerPath(ConsumerName consumerName)
    {
        return new ConsumerPath(consumerName);
    }
    
    private static string SplitOnUpperCase(ConsumerName name)
    {
        //Add '-' between UpperCases and set everything to lower : TestTopic -> test-topic
        var nameBuildingResult = new StringBuilder();

        foreach (var ch in name.Name)
        {
            if (char.IsUpper(ch) && nameBuildingResult.Length > 0)
            {
                nameBuildingResult.Append('-');
            }

            nameBuildingResult.Append(ch);
        }

        return nameBuildingResult.ToString();
    }
}