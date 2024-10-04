using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MicrosoftOrleansBasicExample.Common;
using MicrosoftOrleansBasicExample.Grains.Interfaces;
using MicrosoftOrleansBasicExample.Grains.Interfaces.Observers;

namespace MicrosoftOrleansBasicExample.Client.Components.Pages
{
    public partial class Chat : IReceiveMessage, IAsyncDisposable
    {
        [Inject]
        public required IClusterClient ClusterClient { get; set; }

        [Inject]
        public required ILocalStorageService LocalStorage { get; set; }

        [Inject]
        public required IJSRuntime Js { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        [Parameter]
        public required string Chatroom { get; set; }

        private string username = string.Empty;
        private List<ChatMessage> messages = new();
        private string draftMessage = string.Empty;

        private IReceiveMessage? chatroomClientReference;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
            {
                return;
            }

            // We have to do this in the OnAfterRenderAsync method since we run serverside
            // and cant't use the JSRuntime and localStorage in the OnInitializedAsync method
            await SetUsernameAsync();
            await SubscribeToChatroomAsync();
        }

        private async Task SetUsernameAsync()
        {
            var userGuid = await LocalStorage.GetItemAsync<Guid>("userGuid");
            if (userGuid == Guid.Empty)
            {
                // Return to home
                NavigationManager.NavigateTo(string.Empty);
                return;
            }

            var user = ClusterClient.GetGrain<IUser>(userGuid);
            username = await user.GetUsernameAsync() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(username))
            {
                NavigationManager.NavigateTo(string.Empty);
            }
        }

        private async Task SubscribeToChatroomAsync()
        {
            var chatroomGrain = ClusterClient.GetGrain<IChatroom>(Chatroom.ToLower());

            messages = await chatroomGrain.GetMessageHistory();

            chatroomClientReference = ClusterClient.CreateObjectReference<IReceiveMessage>(this);
            await chatroomGrain.JoinAsync(chatroomClientReference, username);
        }

        /// <summary>
        /// The receive message method that will be called by the chatroom grain
        /// This method must be void since Microsoft Orleans cant't handle async observers
        /// </summary>
        /// <param name="message">The new message</param>
        public void ReceiveMessage(ChatMessage message)
        {
            messages.Add(message);
            InvokeAsync(StateHasChanged).Wait();
            Js.InvokeVoidAsync(identifier: "updateScroll", args: null).AsTask().Wait();
        }

        private async Task KeyUpSendMessageAsync(KeyboardEventArgs args)
        {
            if (args.Code == "Enter" || args.Code == "NumpadEnter")
            {
                await SendMessageAsync();
            }
        }

        private async Task SendMessageAsync()
        {
            var chatroomGrain = ClusterClient.GetGrain<IChatroom>(Chatroom);

            await chatroomGrain.PublishAsync(draftMessage, username);

            // Reset the message
            draftMessage = string.Empty;
            await InvokeAsync(StateHasChanged);
        }

        public async ValueTask DisposeAsync()
        {
            if (chatroomClientReference is null)
            {
                return;
            }

            // If the chat gets disposed, it means the page is closed
            // Leave the chat
            var chatroomGrain = ClusterClient.GetGrain<IChatroom>(Chatroom);
            await chatroomGrain.LeaveAsync(chatroomClientReference, username);
        }

    }
}
