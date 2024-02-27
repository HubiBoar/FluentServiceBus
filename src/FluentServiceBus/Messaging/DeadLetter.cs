using Azure.Messaging.ServiceBus;
using System.Text;

namespace FluentServiceBus;

internal sealed class DeadLetterMessageHelper
{
    public static async Task Message(
        ProcessMessageEventArgs client,
        DeadLetter error)
    {
        var properties = new Dictionary<string, object>();

        AddDeadLetterProperties(error, properties);

        await client.DeadLetterMessageAsync(client.Message, properties);
    }
    
    private static void AddDeadLetterProperties(DeadLetter error, IDictionary<string, object> properties)
    {
        var message = TrimToByteLength(error.Message, 32000);

        properties.Add("DeadLetterErrorMessage", message);
    }
    
    private static string TrimToByteLength(string input, int byteLength)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var currentBytes = Encoding.UTF8.GetByteCount(input);
        if (currentBytes <= byteLength)
        {
            return input;
        }

        //Are we dealing with all 1-byte chars? Use substring(). This cuts the time in half.
        if (currentBytes == input.Length)
        {
            return input.Substring(0, byteLength);
        }

        var bytesArray = Encoding.UTF8.GetBytes(input);
        Array.Resize(ref bytesArray, byteLength);
        var wordTrimmed = Encoding.UTF8.GetString(bytesArray, 0, byteLength);

        //If a multi-byte sequence was cut apart at the end, the decoder will put a replacement character '�'
        //so trim off the potential trailing '�'
        return wordTrimmed.TrimEnd('�');
    }
}