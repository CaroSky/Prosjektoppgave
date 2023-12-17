using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Blazor.Pages;
using Microsoft.Extensions.Logging;
using SharedModels.Entities;
using SharedModels.ViewModels;
using Post = SharedModels.Entities.Post;

namespace Blazor.Data
{
    public class NotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NotificationService> _logger;
        private readonly CustomAuthenticationStateProvider _authenticationStateProvider;
        private string _token;
        public event Action<int> OnNotificationCountChanged;
        public int notificationCount = 0;


        public NotificationService(HttpClient httpClient, ILogger<NotificationService> logger, CustomAuthenticationStateProvider AuthenticationStateProvider)
        {
            _logger = logger;
            _authenticationStateProvider = AuthenticationStateProvider;
            _token = _authenticationStateProvider._tokenService.JwtToken;
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }

 

        public async Task<IEnumerable<Post>> GetNotificationPosts()
        {
            _logger.LogInformation($"Sending HTTP GET request to URL: api/notification/");

            var response = await _httpClient.GetAsync($"api/notification");
            _logger.LogInformation($"Received HTTP response with status code: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode} and content {errorContent}");
            }

            try
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<Post>>();
            }
            catch (JsonException ex)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error parsing JSON response: {responseContent}", ex);
                throw;
            }
        }

        public async Task<int> GetNotificationsCount()
        {
            _logger.LogInformation($"Sending HTTP GET request to URL: api/notification/count");

            var response = await _httpClient.GetAsync($"api/notification/count");
            _logger.LogInformation($"Received HTTP response with status code: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode} and content {errorContent}");
            }

            try
            {
                notificationCount = await response.Content.ReadFromJsonAsync<int>();
                OnNotificationCountChanged?.Invoke(notificationCount);
                return notificationCount;
            }
            catch (JsonException ex)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error parsing JSON response: {responseContent}", ex);
                throw;
            }
        }

        public async Task<bool> DeleteNotification(int postId)
        {
            _logger.LogInformation($"Sending HTTP DELETE request to URL: api/notification");

            var response = await _httpClient.DeleteAsync($"api/notification/{postId}");
            _logger.LogInformation($"Received HTTP response with status code: {response.StatusCode}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAllNotificationForUser()
        {
            _logger.LogInformation($"Sending HTTP DELETE request to URL: api/notification");

            var response = await _httpClient.DeleteAsync($"api/notification");
            _logger.LogInformation($"Received HTTP response with status code: {response.StatusCode}");
            return response.IsSuccessStatusCode;
        }

    }


}



