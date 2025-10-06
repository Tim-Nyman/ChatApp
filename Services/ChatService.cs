using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatApp.Services
{
    public class ChatService
    {
        private readonly HubConnection _hubConnection;

        public event Action<string, string>? OnMessageReceived;
        public event Action<string>? OnUserJoined;
        public async Task StartAsync() => await _hubConnection.StartAsync();

        public ChatService(NavigationManager nav)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(nav.ToAbsoluteUri("/chatHub"))
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                OnMessageReceived?.Invoke(user, message);
            });
        }

        public async Task SendMessageAsync(string user, string message)
            => await _hubConnection.SendAsync("SendMessage", user, message);

        public bool IsConnected => _hubConnection.State == HubConnectionState.Connected;
    }
}
