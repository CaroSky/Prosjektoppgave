using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Blazor.Pages;
using Microsoft.Extensions.Logging;
using SharedModels.Entities;
using SharedModels.ViewModels;

namespace Blazor.Data
{
    public class PostService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PostService> _logger;
        private readonly CustomAuthenticationStateProvider _authenticationStateProvider;
        private string _token;


        public PostService(HttpClient httpClient, ILogger<PostService> logger, CustomAuthenticationStateProvider AuthenticationStateProvider)
        {
            _logger = logger;
            _authenticationStateProvider = AuthenticationStateProvider;
            _token = _authenticationStateProvider._tokenService.JwtToken;
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }

        public async Task<PostIndexViewModel> GetPostsByBlogIdAsync(int blogId)

        {
            _logger.LogInformation($"Sending HTTP GET request to URL: api/post/{blogId}/posts");

            var response = await _httpClient.GetAsync($"api/post/{blogId}/posts");
            _logger.LogInformation($"Received HTTP response with status code: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode} and content {errorContent}");
            }

            try
            {
                return await response.Content.ReadFromJsonAsync<PostIndexViewModel>();
            }
            catch (JsonException ex)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error parsing JSON response: {responseContent}", ex);
                throw;
            }
        }

        // Andre metoder for å opprette, oppdatere, og slette poster

        public async Task<bool> CreatePostAsync(PostCreateViewModel newPost)
        {
            _logger.LogInformation("Sending request to create new post");
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/post", newPost);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("New post created successfully");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error creating post: {errorContent}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request exception: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> UpdatePostAsync(int postId, PostEditViewModel updatedPost)
        {
            _logger.LogInformation($"Sending request to update post with ID {postId}");
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/post/{postId}", updatedPost);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Post with ID {postId} updated successfully");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error updating post: {errorContent}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeletePostAsync(int postId)
        {
            var response = await _httpClient.DeleteAsync($"api/post/{postId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<PostIndexViewModel> GetPostByPostIdAsync(int postId)

        {
            _logger.LogInformation($"Sending HTTP GET request to URL: api/post/{postId}");

            var response = await _httpClient.GetAsync($"api/post/{postId}");
            _logger.LogInformation($"Received HTTP response with status code: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode} and content {errorContent}");
            }

            try
            {
                return await response.Content.ReadFromJsonAsync<PostIndexViewModel>();
            }
            catch (JsonException ex)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error parsing JSON response: {responseContent}", ex);
                throw;
            }
        }

        public async Task<bool> CreateLikeAsync(int postId)
        {
            _logger.LogInformation("Sending request to create new like");
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/like", postId);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("New like created successfully");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error creating like: {errorContent}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteLikeAsync(int postId)
        {
            var response = await _httpClient.DeleteAsync($"api/like/{postId}");
            return response.IsSuccessStatusCode;
        }

        //public async Task<IEnumerable<Notfication>> GetNotifications()
        //{
        //    _logger.LogInformation($"Sending HTTP GET request to URL: api/notification/");

        //    var response = await _httpClient.GetAsync($"api/notification/count");
        //    _logger.LogInformation($"Received HTTP response with status code: {response.StatusCode}");

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        var errorContent = await response.Content.ReadAsStringAsync();
        //        throw new HttpRequestException($"Request failed with status code {response.StatusCode} and content {errorContent}");
        //    }

        //    try
        //    {
        //        return await response.Content.ReadFromJsonAsync<IEnumerable<Notfication>>();
        //    }
        //    catch (JsonException ex)
        //    {
        //        var responseContent = await response.Content.ReadAsStringAsync();
        //        _logger.LogError($"Error parsing JSON response: {responseContent}", ex);
        //        throw;
        //    }
        //}

        //public async Task<int> GetNotificationsCount()
        //{
        //    _logger.LogInformation($"Sending HTTP GET request to URL: api/notification/count");

        //    var response = await _httpClient.GetAsync($"api/notification/count");
        //    _logger.LogInformation($"Received HTTP response with status code: {response.StatusCode}");

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        var errorContent = await response.Content.ReadAsStringAsync();
        //        throw new HttpRequestException($"Request failed with status code {response.StatusCode} and content {errorContent}");
        //    }

        //    try
        //    {
        //        return await response.Content.ReadFromJsonAsync<int>();
        //    }
        //    catch (JsonException ex)
        //    {
        //        var responseContent = await response.Content.ReadAsStringAsync();
        //        _logger.LogError($"Error parsing JSON response: {responseContent}", ex);
        //        throw;
        //    }
        //}

        //public async Task UpdateNotificationCount()
        //{
        //    // Update the count (this might be triggered after a database change)
        //    notificationCount = await GetNotificationsCount();

        //    // Notify subscribers (e.g., components interested in the notification count)
        //    OnNotificationCountChanged?.Invoke(notificationCount);
        //}
    }


}



