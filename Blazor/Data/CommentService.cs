using Microsoft.AspNetCore.Mvc;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharedModels.ViewModels;
using SharedModels.Entities;
using System.Text.Json;


namespace Blazor.Data
{
    public class CommentService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CommentService> _logger;

        public CommentService(HttpClient httpClient, ILogger<CommentService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<CommentIndexViewModel> GetCommentsByPostIdAsync(int postId)
        {
            _logger.LogInformation($"Sending HTTP GET request to URL: api/comment/{postId}/comments");

            var response = await _httpClient.GetAsync($"api/comment/{postId}/comments");
            _logger.LogInformation($"Received HTTP response with status code: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode} and content {errorContent}");
            }

            try
            {
                var commentIndexViewModel = await response.Content.ReadFromJsonAsync<CommentIndexViewModel>();
                return commentIndexViewModel;
            }
            catch (JsonException ex)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error parsing JSON response: {responseContent}", ex);
                throw;
            }
        }

        // Implementer andre nødvendige metoder her (CreateCommentAsync, UpdateCommentAsync, DeleteCommentAsync, osv.)
    }
}

