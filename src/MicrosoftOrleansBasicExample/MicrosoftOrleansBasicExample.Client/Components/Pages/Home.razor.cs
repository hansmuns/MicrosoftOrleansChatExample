using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MicrosoftOrleansBasicExample.Grains.Interfaces;

namespace MicrosoftOrleansBasicExample.Client.Components.Pages
{
    public partial class Home
    {
        [Inject]
        public required IClusterClient clusterClient { get; set; }

        [Inject]
        public required ILocalStorageService localStorage { get; set; }

        [Inject]
        public required NavigationManager navigationManager { get; set; }

        private List<string> chatrooms = new();
        private readonly Random random = new();
        private string username = string.Empty;
        private Guid userGuid = Guid.Empty;
        private string chatroomName = string.Empty;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
            {
                return;
            }

            await SetCorrectUserGuidAsync();
            await SetCorrectUsernameAsync();
            await LoadChatroomsAsync();

            StateHasChanged();
        }

        private async Task SetCorrectUserGuidAsync()
        {
            userGuid = await localStorage.GetItemAsync<Guid>("userGuid");
            if (userGuid == Guid.Empty)
            {
                userGuid = Guid.NewGuid();
                await localStorage.SetItemAsync("userGuid", userGuid);
            }
        }

        private async Task SetCorrectUsernameAsync()
        {
            // Ensure the user exists with the username
            var user = clusterClient.GetGrain<IUser>(userGuid);
            var currentUsername = await user.GetUsernameAsync();
            if (string.IsNullOrWhiteSpace(currentUsername))
            {
                username = GenerateRandomUsername();
                await user.SetUsernameAsync(username);
            }
            else
            {
                username = currentUsername;
            }
        }
        private string GenerateRandomUsername()
        {
            string[] adjectives = { "Quick", "Lazy", "Happy", "Sad", "Brave", "Clever", "Bold", "Shy", "Calm", "Eager" };
            string[] nouns = { "Fox", "Dog", "Cat", "Mouse", "Bear", "Lion", "Tiger", "Wolf", "Eagle", "Shark" };
            return $"{adjectives[random.Next(adjectives.Length)]}{nouns[random.Next(nouns.Length)]}{random.Next(1000, 9999)}";
        }

        private async Task LoadChatroomsAsync()
        {
            var user = clusterClient.GetGrain<IUser>(userGuid);
            chatrooms = await user.GetChatroomsAsync();
        }

        private async Task JoinChatAsync(string chatroom)
        {
            chatroom = chatroom.ToLower();

            var userGrain = clusterClient.GetGrain<IUser>(userGuid);
            await userGrain.JoinChatroomAsync(chatroom);
            navigationManager.NavigateTo($"chats/{chatroom}");
        }

        private async Task KeyUpCreateChatAsync(KeyboardEventArgs args)
        {
            if (args.Code == "Enter" || args.Code == "NumpadEnter")
            {
                await CreateChatAsync();
            }
        }

        private async Task CreateChatAsync()
        {
            if (string.IsNullOrWhiteSpace(chatroomName))
            {
                return;
            }

            var userGrain = clusterClient.GetGrain<IUser>(userGuid);
            await userGrain.JoinChatroomAsync(chatroomName.ToLower());
            navigationManager.NavigateTo($"chats/{chatroomName.ToLower()}");
        }
    }
}
