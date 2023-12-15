using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Blazor.Data
{
    public class SignalRService
    {
        private readonly ILogger<SignalRService> _logger;
        private readonly CustomAuthenticationStateProvider _authenticationStateProvider;
        private HubConnection _hubConnection;
        private TaskCompletionSource<string> _tcs = new TaskCompletionSource<string>();
        public event Action<string> OnMessageReceived;
        private string _token;
        private List<string> toasts = new List<string>();
        private TaskCompletionSource<string> _messageTaskSource = new TaskCompletionSource<string>();

        public SignalRService(ILogger<SignalRService> logger, CustomAuthenticationStateProvider AuthenticationStateProvider)
        {
            _logger = logger;
            _authenticationStateProvider = AuthenticationStateProvider;
           
        }

        public async Task InitializeConnection()
        {
         

            _token = _authenticationStateProvider._tokenService.JwtToken;
            _logger.LogInformation($"Initializing SignalR Connection with token: {_token}");
            if (!string.IsNullOrEmpty(_token))
            {
                _hubConnection?.DisposeAsync();
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl("https://localhost:5001/notificationhub", options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(_token);
                    })
                    .Build();


                _hubConnection.On<string>("ReceiveTagNotification", (message) =>
                {
                    _logger.LogInformation($"Notifikasjon mottatt: {message}");
                    _tcs.TrySetResult(message);
                });


                try
                {
                    await _hubConnection.StartAsync();
                    _logger.LogInformation("SignalR-tilkobling startet");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Feil under initialisering av SignalR-tilkobling: {ex.Message}");
                }
            }
            else
            {
                _logger.LogWarning("Token er null eller tom, kan ikke opprette hub-tilkobling");
            }
        }
        public void RegisterMessageHandler(Action<string> handler)
        {
            OnMessageReceived += handler;
            _logger.LogInformation($"Handler registered for OnMessageReceived.");
        }

        public void UnregisterMessageHandler(Action<string> handler)
        {
            _logger.LogInformation($"Handler unregistered for OnMessageReceived.");
            OnMessageReceived -= handler;
        }

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
        public async Task Disconnect()
        {
            if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
                _logger.LogInformation("SignalR-tilkobling avsluttet");
            }
        }
        public async Task<string> WaitForMessageAsync()
        {
            var message = await _tcs.Task;
            _tcs = new TaskCompletionSource<string>();
            _logger.LogError($"Feil i WaitForMessageAsync: {message}");
            return message;
        }

    }
}
