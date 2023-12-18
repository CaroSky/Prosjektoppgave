using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace Blazor.Data
{
    public class SignalRService
    {
        private readonly CustomAuthenticationStateProvider _authenticationStateProvider;
        private HubConnection _hubConnection;
        private TaskCompletionSource<string> _tcs = new TaskCompletionSource<string>();
        private string _token;

        public SignalRService(CustomAuthenticationStateProvider AuthenticationStateProvider)
        {
            _authenticationStateProvider = AuthenticationStateProvider;
        }

        public async Task InitializeConnection()
        {
            _token = _authenticationStateProvider._tokenService.JwtToken;

            if (!string.IsNullOrEmpty(_token))
            {
                _hubConnection?.DisposeAsync();
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl("https://localhost:5001/subscriptionHub", options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(_token);
                    })
                    .Build();

                _hubConnection.On<string>("ReceiveSubscriptionMessage", (message) =>
                {
                    _tcs.TrySetResult(message);
                });

                try
                {
                    await _hubConnection.StartAsync();
                }
                catch (Exception)
                {
                    // Handle exception here
                }
            }
        }

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public async Task Disconnect()
        {
            if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
            }
        }

        public async Task<string> WaitForMessageAsync()
        {
            var message = await _tcs.Task;
            _tcs = new TaskCompletionSource<string>();
            return message;
        }
    }
}
