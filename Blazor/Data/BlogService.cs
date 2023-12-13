using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using NuGet.Protocol.Core.Types;
using SharedModels.Entities;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Blazor.Data
{
    public class BlogService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BlogService> _logger;
        private readonly CustomAuthenticationStateProvider _authenticationStateProvider;
        private string _token;
        private readonly UserManager<IdentityUser> _userManager;

        public BlogService(HttpClient httpClient, ILogger<BlogService> logger, CustomAuthenticationStateProvider AuthenticationStateProvider, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _authenticationStateProvider = AuthenticationStateProvider;
            _token = _authenticationStateProvider._tokenService.JwtToken;
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            _userManager = userManager;


        }
        public async Task<string> GetUsernameByOwnerIdAsync(string ownerId)
        {
            var user = await _userManager.FindByIdAsync(ownerId);
            if (user != null)
            {
                return user.UserName;
            }
            return null;
        }


        public async Task<IEnumerable<Blog>> GetBlogsAsync()
        {
            _logger.LogInformation($"Sending HTTP GET request to URL: {"api/blog"}");
            var response = await _httpClient.GetAsync("api/blog");
            _logger.LogInformation("Sending request to get all blogs");
            _logger.LogInformation($"Received HTTP response with status code: {response.StatusCode}");
            foreach (var header in response.Headers)
            {
                _logger.LogInformation($"Header: {header.Key} Value: {string.Join(", ", header.Value)}");
            }
            // _logger.LogError($"Error response content: {errorContent}");



            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode} and content {errorContent}");
            }

            try
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<Blog>>();
            }
            catch (JsonException ex)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Response content: {responseContent}");

                throw new JsonException("Error parsing JSON response", ex);
            }
        }

        public async Task<Blog> GetBlogByIdAsync(int blogId)
        {
            _logger.LogInformation($"Sending HTTP GET request to URL: api/blog/{blogId}");

            var response = await _httpClient.GetAsync($"api/blog/{blogId}");
            _logger.LogInformation($"Received HTTP response with status code: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error response content: {errorContent}");
                throw new HttpRequestException($"Request failed with status code {response.StatusCode} and content {errorContent}");
            }

            try
            {
                return await response.Content.ReadFromJsonAsync<Blog>();
            }
            catch (JsonException ex)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Response content: {responseContent}");
                throw new JsonException("Error parsing JSON response", ex);
            }
        }




        // Andre metoder for å opprette, oppdatere, og slette blogginnlegg

        public async Task<Blog> PutBlogAsync(Blog blog)
        {
            _logger.LogInformation($"Sending HTTP PUT request to URL: {"api/blog"}");

            // Construct the URL with the parameter
            string apiUrl = $"api/blog/{blog.BlogId}";

            var response = await _httpClient.PutAsJsonAsync(apiUrl, blog);
            _logger.LogInformation("Sending request to update a blog");
            _logger.LogInformation($"Received HTTP response with status code: {response.StatusCode}");
            foreach (var header in response.Headers)
            {
                _logger.LogInformation($"Header: {header.Key} Value: {string.Join(", ", header.Value)}");
            }
            // _logger.LogError($"Error response content: {errorContent}");



            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode} and content {errorContent}");
            }
            try
            {
                return await response.Content.ReadFromJsonAsync<Blog>();
            }
            catch (JsonException ex)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Response content: {responseContent}");

                throw new JsonException("Error parsing JSON response", ex);
            }
        }

        public async Task<Blog> PostBlogAsync(Blog blog)
        {
            _logger.LogInformation($"Sending HTTP POST request to URL: {"api/blog"}");

            // Construct the URL with the parameter
            string apiUrl = $"api/blog/";

            var response = await _httpClient.PostAsJsonAsync(apiUrl, blog);
            _logger.LogInformation("Sending request to post a blog");
            _logger.LogInformation($"Received HTTP response with status code: {response.StatusCode}");
            foreach (var header in response.Headers)
            {
                _logger.LogInformation($"Header: {header.Key} Value: {string.Join(", ", header.Value)}");
            }
            // _logger.LogError($"Error response content: {errorContent}");



            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode} and content {errorContent}");
            }
            try
            {
                return await response.Content.ReadFromJsonAsync<Blog>();
            }
            catch (JsonException ex)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Response content: {responseContent}");

                throw new JsonException("Error parsing JSON response", ex);
            }
        }

        public async Task<Blog> DeleteBlogAsync(Blog blog)
        {
            _logger.LogInformation($"Sending HTTP DELETE request to URL: {"api/blog"}");

            // Construct the URL with the parameter
            string apiUrl = $"api/blog/{blog.BlogId}";

            var response = await _httpClient.DeleteAsync(apiUrl);
            _logger.LogInformation("Sending request to delete a blog");
            _logger.LogInformation($"Received HTTP response with status code: {response.StatusCode}");
            foreach (var header in response.Headers)
            {
                _logger.LogInformation($"Header: {header.Key} Value: {string.Join(", ", header.Value)}");
            }
            // _logger.LogError($"Error response content: {errorContent}");



            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode} and content {errorContent}");
            }
            try
            {
                return await response.Content.ReadFromJsonAsync<Blog>();
            }
            catch (JsonException ex)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Response content: {responseContent}");

                throw new JsonException("Error parsing JSON response", ex);
            }
        }
        public async Task SubscribeToBlogAsync(int blogId)
        {
            _logger.LogInformation($"Sending HTTP POST request to subscribe to blog with ID: {blogId}");

            var response = await _httpClient.PostAsync($"api/blog/{blogId}/subscribe", null); // Assuming no body is required
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error response content: {errorContent}");
                throw new HttpRequestException($"Request failed with status code {response.StatusCode} and content {errorContent}");
            }

            _logger.LogInformation($"Subscribed to blog with ID: {blogId}");
        }

        public async Task UnsubscribeFromBlogAsync(int blogId)
        {
            _logger.LogInformation($"Sending HTTP POST request to unsubscribe from blog with ID: {blogId}");

            var response = await _httpClient.PostAsync($"api/blog/{blogId}/unsubscribe", null); // Assuming no body is required
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error response content: {errorContent}");
                throw new HttpRequestException($"Request failed with status code {response.StatusCode} and content {errorContent}");
            }

            _logger.LogInformation($"Unsubscribed from blog with ID: {blogId}");
        }

        public async Task<bool> IsSubscribedAsync(int blogId)
        {
            _logger.LogInformation($"Sending HTTP GET request to check subscription status for blog with ID: {blogId}");

            var response = await _httpClient.GetAsync($"api/blog/{blogId}/isSubscribed");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error response content: {errorContent}");
                throw new HttpRequestException($"Request failed with status code {response.StatusCode} and content {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return bool.Parse(responseContent); 
        }
        public async Task<Dictionary<int, bool>> GetAllSubscriptionStatusesAsync()
        {
            _token = _authenticationStateProvider._tokenService.JwtToken;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            _logger.LogInformation("Sending HTTP GET request to retrieve all blog subscription statuses.");

            var response = await _httpClient.GetAsync("api/blog/subscriptionStatuses"); 

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error fetching subscription statuses: {errorContent}");
                throw new HttpRequestException($"Request failed with status code {response.StatusCode} and content {errorContent}");
            }

            _logger.LogInformation("Received subscription statuses successfully.");

            var responseContent = await response.Content.ReadAsStringAsync();
            try
            {
                var subscriptionStatuses = JsonSerializer.Deserialize<Dictionary<int, bool>>(responseContent);
                return subscriptionStatuses ?? new Dictionary<int, bool>();
            }
            catch (JsonException ex)
            {
                _logger.LogError($"Error parsing subscription statuses JSON: {ex}");
                throw;
            }
        }
        public async Task<List<String>> SearchSuggestions(String _searchQuery)
        {

            string apiUrl = $"api/search/suggestions/{_searchQuery}";
            var response = await _httpClient.GetAsync(apiUrl);


            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode} and content {errorContent}");
            }
            try
            {
                return await response.Content.ReadFromJsonAsync<List<String>>();
            }
            catch (JsonException ex)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                throw new JsonException("Error parsing JSON response", ex);
            }
        }

    }
}
