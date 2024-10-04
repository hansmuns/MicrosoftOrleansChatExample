namespace MicrosoftOrleansBasicExample.Common
{
    [GenerateSerializer]
    public record ChatMessage(string Message, string Username, DateTimeOffset DateTimeSend);
}
