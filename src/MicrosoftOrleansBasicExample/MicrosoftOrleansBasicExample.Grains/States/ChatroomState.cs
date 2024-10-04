using MicrosoftOrleansBasicExample.Common;

namespace MicrosoftOrleansBasicExample.Grains.States
{
    public class ChatroomState
    {
        public List<ChatMessage> ChatMessages { get; set; } = new();
    }
}
