namespace MicrosoftOrleansBasicExample.Grains.States
{
    public class UserState
    {
        public string? Username { get; set; }

        public List<string> Chatrooms { get; set; } = new();
    }
}
