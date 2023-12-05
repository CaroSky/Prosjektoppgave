using SharedModels.Entities;
using System.Text.Json;


namespace Blazor.Data
{
    public class SearchService
    {

        private readonly HttpClient _httpClient;
        private readonly ILogger<BlogService> _logger;

        public SearchService(HttpClient httpClient, ILogger<BlogService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<Post>> SearchPostAsync(string tag)
        {
            _logger.LogInformation($"Sending HTTP GET request to URL: {"api/search"}");

            //var response = await _httpClient.GetAsync("api/search/");
            var response = await _httpClient.GetAsync($"api/search/{tag}");
            _logger.LogInformation("Sending request to get all posts with tag");
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
                return await response.Content.ReadFromJsonAsync<IEnumerable<Post>>();
            }
            catch (JsonException ex)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Response content: {responseContent}");

                throw new JsonException("Error parsing JSON response", ex);
            }
        }
    }
}
