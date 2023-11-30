using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedModels.Entities;
using System.Text.Json;

namespace Blazor.Data
{
    public class BlogService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BlogService> _logger;

        public BlogService(HttpClient httpClient, ILogger<BlogService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<Blog>> GetBlogsAsync()
        {
            var response = await _httpClient.GetAsync("api/blog");
            _logger.LogInformation("Sending request to get all blogs");

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
                throw new JsonException("Error parsing JSON response", ex);
            }
        }


        // Andre metoder for å opprette, oppdatere, og slette blogginnlegg
    }

}
