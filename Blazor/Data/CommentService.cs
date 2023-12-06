using Microsoft.AspNetCore.Mvc;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharedModels.ViewModels;
using SharedModels.Entities;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;


namespace Blazor.Data
{
    public class CommentService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CommentService> _logger;
        private readonly CustomAuthenticationStateProvider _authenticationStateProvider;
        private string _token;

        public CommentService(HttpClient httpClient, ILogger<CommentService> logger, CustomAuthenticationStateProvider AuthenticationStateProvider)
        {
            _logger = logger;
            _authenticationStateProvider = AuthenticationStateProvider;
            _token = _authenticationStateProvider._tokenService.JwtToken;
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
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

        public async Task<bool> CreateCommentAsync(CommentCreateViewModel newComment)
        {
            _logger.LogInformation("Sending request to create new comment");
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/comment", newComment);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("New comment created successfully");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error creating comment: {errorContent}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request exception: {ex.Message}");
                return false;
            }
        }
        public async Task<CommentEditViewModel> GetCommentAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/comment/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CommentEditViewModel>();
            }
            else
            {
                // Håndter feil
                return null;
            }
        }

        public async Task<bool> UpdateCommentAsync(int id, CommentEditViewModel updatedComment)
        {
            _logger.LogInformation($"Sender oppdateringsforespørsel for kommentar med ID {id}");
            var response = await _httpClient.PutAsJsonAsync($"api/comment/{id}", updatedComment);
            _logger.LogInformation($"Responsstatus for oppdatering: {response.StatusCode}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteCommentAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/comment/{id}");
            return response.IsSuccessStatusCode;
        }
    }

}

