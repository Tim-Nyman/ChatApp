using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace ChatApp.Services
{
    public class ChatService
    {
        private readonly HubConnection _hubConnection;
        private readonly IJSRuntime _jsRuntime;
        public event Action<string, string>? OnMessageReceived;

        public async Task StartAsync() => await _hubConnection.StartAsync();

        public ChatService(NavigationManager nav, IJSRuntime js)
        {
            _jsRuntime = js;
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(nav.ToAbsoluteUri("/chatHub"), options =>
                {
                    options.AccessTokenProvider = async () =>
                    {
                        return await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "jwtToken");
                    };
                })
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                OnMessageReceived?.Invoke(user, message);
            });
        }

        public async Task SendMessageAsync(string user, string message)
            => await _hubConnection.SendAsync("SendMessage", user, message);
    }
}
