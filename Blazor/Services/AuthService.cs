using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging; // Import the logging namespace
using SharedModels.Entities;

namespace Blazor.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly ILogger<AuthService> _logger; // Define a logger
        private readonly ApiAuthenticationStateProvider _authenticationStateProvider;

        public AuthService(HttpClient httpClient,
                           ApiAuthenticationStateProvider authenticationStateProvider,
                           ILocalStorageService localStorage,
                           ILogger<AuthService> logger) // Inject the logger
        {
            _httpClient = httpClient;
            _authenticationStateProvider = authenticationStateProvider;
            _localStorage = localStorage;
            _logger = logger; // Assign the logger
        }

        public async Task<RegisterResult> Register(RegisterModel registerModel)
        {
            // Logging for debugging purposes
            _logger.LogInformation("Registering user: {Email}", registerModel.Email);

            var registerAsJson = JsonSerializer.Serialize(registerModel);
            var content = new StringContent(registerAsJson, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/accounts", content);

            // Logging the response status code
            _logger.LogInformation("Response status code: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                // Handle the error response here if needed.
                _logger.LogError("Registration failed with status code: {StatusCode}", response.StatusCode);
                return null;
            }

            var resultJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RegisterResult>(resultJson);

            return result;
        }

        public async Task<LoginResult> Login(LoginModel loginModel)
        {
            // Logging for debugging purposes
            _logger.LogInformation("Logging in user: {Email}", loginModel.Email);

            var loginAsJson = JsonSerializer.Serialize(loginModel);
            var content = new StringContent(loginAsJson, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("api/Login", content);

                // Logging the response status code
                _logger.LogInformation("Response status code: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var loginResultJson = await response.Content.ReadAsStringAsync();
                    var loginResult = JsonSerializer.Deserialize<LoginResult>(loginResultJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (loginResult != null)
                    {
                        await _localStorage.SetItemAsync("authToken", loginResult.Token);
                        ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(loginModel.Email);
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);
                    }

                    return loginResult;
                }
                else
                {
                    // Handle non-successful response (e.g., display error message)
                    _logger.LogError("Login failed with status code: {StatusCode}", response.StatusCode);
                    return new LoginResult { Successful = false, Error = "Login failed." };
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions here
                _logger.LogError(ex, "An error occurred during login.");
                return new LoginResult { Successful = false, Error = "An error occurred during login." };
            }
        }

        public async Task Logout()
        {
            // Logging for debugging purposes
            _logger.LogInformation("Logging out user");

            await _localStorage.RemoveItemAsync("authToken");
            ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}
