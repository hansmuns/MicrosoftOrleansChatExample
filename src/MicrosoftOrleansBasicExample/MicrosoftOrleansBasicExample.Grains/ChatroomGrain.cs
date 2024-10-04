using Microsoft.Extensions.Logging;
using MicrosoftOrleansBasicExample.Common;
using MicrosoftOrleansBasicExample.Grains.Interfaces;
using MicrosoftOrleansBasicExample.Grains.Interfaces.Observers;
using MicrosoftOrleansBasicExample.Grains.States;

namespace MicrosoftOrleansBasicExample.Grains
{
    public class ChatroomGrain : Grain, IChatroom
    {
        private readonly List<IReceiveMessage> observers = new();

        private readonly IPersistentState<ChatroomState> chatroom;
        private readonly ILogger logger;

        public ChatroomGrain(ILogger<ChatroomGrain> logger,
                             [PersistentState(stateName: "Chatroom", storageName: "azure")] IPersistentState<ChatroomState> chatroom)
        {
            this.logger = logger;
            this.chatroom = chatroom;
        }

        public async ValueTask JoinAsync(IReceiveMessage observer, string username)
        {
            logger.LogInformation($"{username} joined the chatroom '{this.GetPrimaryKeyString()}'");

            observers.Add(observer);

            await PublishAsync($"User {username} joined the chat :D", "System");
        }

        public async ValueTask LeaveAsync(IReceiveMessage observer, string username)
        {
            logger.LogInformation($"{username} left the chatroom '{this.GetPrimaryKeyString()}'");
            observers.Remove(observer);

            await PublishAsync($"User {username} left the chat >:(", "System");
        }

        public Task PublishAsync(string message, string username)
        {
            var chatMessage = new ChatMessage(message, username, DateTimeOffset.UtcNow);
            logger.LogInformation($"{username} send '{chatMessage.Message}' at {chatMessage.DateTimeSend} in '{this.GetPrimaryKeyString()}'");

            chatroom.State.ChatMessages.Add(chatMessage);
            foreach (var observer in observers)
            {
                observer.ReceiveMessage(chatMessage);
            }

            return Task.CompletedTask;
        }

        public ValueTask<List<ChatMessage>> GetMessageHistory()
        {
            return ValueTask.FromResult(chatroom.State.ChatMessages.OrderBy(x => x.DateTimeSend).ToList());
        }

        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            // Store the state
            await chatroom.WriteStateAsync();
        }
    }
}
