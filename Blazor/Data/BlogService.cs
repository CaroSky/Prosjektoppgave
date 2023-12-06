using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NuGet.Common;
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

        public BlogService(HttpClient httpClient, ILogger<BlogService> logger, CustomAuthenticationStateProvider AuthenticationStateProvider)
        {
            _logger = logger;
            _authenticationStateProvider = AuthenticationStateProvider;
            _token = _authenticationStateProvider._tokenService.JwtToken;
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);


        }


        //private void ConfigureHttpClient()
        //{
        //    // Create an instance of the JwtTokenHandler and set it as the inner handler for HttpClient
        //    var token = _authenticationStateProvider._tokenService.JwtToken;
        //    var tokenHandler = new JwtTokenHandler(_jwtToken);
        //    var clientWithTokenHandler = new HttpClient(tokenHandler)
        //    {
        //        BaseAddress = new Uri("https://localhost:5001/"), // Set your API endpoint
        //    };
        //    // Inject the configured HttpClient
        //    _httpClient.BaseAddress = clientWithTokenHandler.BaseAddress;
        //    _httpClient.DefaultRequestHeaders.Clear();
        //    _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //}


        public async Task<IEnumerable<Blog>> GetBlogsAsync()
        {
            _logger.LogInformation($"Sending HTTP GET request to URL: {"api/blog"}");
            //ConfigureHttpClient();
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
    }

}
