using MicrosoftOrleansBasicExample.Grains.Interfaces;
using MicrosoftOrleansBasicExample.Grains.States;

namespace MicrosoftOrleansBasicExample.Grains
{
    public class UserGrain : Grain, IUser
    {
        private readonly IPersistentState<UserState> user;

        public UserGrain([PersistentState(stateName: "User", storageName: "azure")] IPersistentState<UserState> user)
        {
            this.user = user;
        }

        public ValueTask JoinChatroomAsync(string chatroom)
        {
            chatroom = chatroom.ToLower();
            if (!user.State.Chatrooms.Contains(chatroom))
            {
                user.State.Chatrooms.Add(chatroom);
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask SetUsernameAsync(string username)
        {
            this.user.State.Username = username;
            return ValueTask.CompletedTask;
        }

        public ValueTask<string?> GetUsernameAsync()
        {
            return new ValueTask<string?>(user.State.Username);
        }

        public ValueTask<List<string>> GetChatroomsAsync()
        {
            return new ValueTask<List<string>>(user.State.Chatrooms);
        }

        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            // store the state
            await user.WriteStateAsync();
        }
    }
}
