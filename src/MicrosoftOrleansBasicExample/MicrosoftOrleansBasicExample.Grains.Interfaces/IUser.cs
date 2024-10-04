namespace MicrosoftOrleansBasicExample.Grains.Interfaces
{
    public interface IUser : IGrainWithGuidKey
    {
        public ValueTask SetUsernameAsync(string username);
        public ValueTask JoinChatroomAsync(string chatroom);
        public ValueTask<string?> GetUsernameAsync();
        public ValueTask<List<string>> GetChatroomsAsync();
    }
}
