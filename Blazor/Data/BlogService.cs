﻿using Microsoft.AspNetCore.Diagnostics;
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


        // Andre metoder for å opprette, oppdatere, og slette blogginnlegg
    }

}
