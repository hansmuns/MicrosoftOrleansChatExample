using MicrosoftOrleansBasicExample.Common;
using MicrosoftOrleansBasicExample.Grains.Interfaces.Observers;

namespace MicrosoftOrleansBasicExample.Grains.Interfaces
{
    public interface IChatroom : IGrainWithStringKey
    {
        public ValueTask JoinAsync(IReceiveMessage observer, string username);
        public ValueTask LeaveAsync(IReceiveMessage observer, string username);
        public Task PublishAsync(string message, string username);
        public ValueTask<List<ChatMessage>> GetMessageHistory();
    }
}
