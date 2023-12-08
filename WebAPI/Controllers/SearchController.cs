using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SharedModels.Entities;
using SharedModels.ViewModels;
using System.Security.Claims;
using WebAPI.Models.Repositories;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : Controller
    {
        private IBlogRepository _repository;

        private UserManager<IdentityUser> _manager;

        private readonly ILogger<SearchController> _logger;


        public SearchController(UserManager<IdentityUser> manager, IBlogRepository repository, ILogger<SearchController> logger)
        {
            this._repository = repository;
            this._manager = manager;
            this._logger = logger;
        }


        [HttpGet("{searchQuery}")]

        public async Task<IActionResult> SearchPosts([FromRoute] string searchQuery)
        {
            _logger.LogInformation("Handling GET search posts");
            try
            {
                var posts = await _repository.SearchPostByTagOrUsername(searchQuery);
                return Ok(posts); // This will return a 200 OK status with the post data
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching posts");
                throw;
            }


        }


        [HttpGet("suggestions/{searchQuery}")]

        public async Task<IActionResult> SearchSuggestions([FromRoute] string searchQuery)
        {
            _logger.LogInformation("Handling GET search suggestions");
            try
            {
                var suggestions = await _repository.SearchSuggestions(searchQuery);
                return Ok(suggestions); // This will return a 200 OK status with the post data
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching suggestions");
                throw;
            }


        }


    }

}
