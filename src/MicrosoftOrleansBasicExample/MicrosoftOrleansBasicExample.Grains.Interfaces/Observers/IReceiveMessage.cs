using MicrosoftOrleansBasicExample.Common;

namespace MicrosoftOrleansBasicExample.Grains.Interfaces.Observers
{
    public interface IReceiveMessage : IGrainObserver
    {
        void ReceiveMessage(ChatMessage message);
    }
}
