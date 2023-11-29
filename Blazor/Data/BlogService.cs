using Microsoft.AspNetCore.Mvc;
using SharedModels.Entities;

namespace Blazor.Data
{
    public class BlogService
    {
        private readonly HttpClient _httpClient;

        public BlogService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<Blog>> GetBlogsAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<Blog>>("api/blog");
        }

        // Andre metoder for å opprette, oppdatere, og slette blogginnlegg
    }

}
