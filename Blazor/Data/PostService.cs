using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
                _logger.LogError($"Request failed with status code {response.StatusCode} and content {errorContent}");
                throw new HttpRequestException($"Request failed with status code {response.StatusCode} and content {errorContent}");
            }

            try
            {
                var postIndexViewModel = await response.Content.ReadFromJsonAsync<PostIndexViewModel>();

                // Log the details of the PostIndexViewModel
                _logger.LogInformation($"Received PostIndexViewModel with {postIndexViewModel.Posts?.Count()} posts");

                foreach (var post in postIndexViewModel.Posts)
                {
                    _logger.LogInformation($"Post ID: {post.PostId}, Liked: {postIndexViewModel.UserLiked.GetValueOrDefault(post.PostId)}");
                }

                return postIndexViewModel;
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

        public async Task<bool> LikePostAsync(int postId)
        {
            _logger.LogInformation($"Sending request to like post with ID {postId}");

            try
            {
                // The token in the header includes the user information
                var response = await _httpClient.PostAsJsonAsync($"api/post/{postId}/like", postId);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Post with ID {postId} liked successfully");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error liking post: {errorContent}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request exception: {ex.Message}");
                return false;
            }
        }

    }

}

