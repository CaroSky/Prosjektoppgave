using System.Net.Http;
using System.Text.Json;
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

        public PostService(HttpClient httpClient, ILogger<PostService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<Post>> GetPostsByBlogIdAsync(int blogId)
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
                var postIndexViewModel = await response.Content.ReadFromJsonAsync<PostIndexViewModel>();
                return postIndexViewModel.Posts;
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

    }


}

