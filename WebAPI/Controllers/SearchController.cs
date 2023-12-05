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

        private string _username = "til061@uit.no";

        private readonly ILogger<SearchController> _logger;


        public SearchController(UserManager<IdentityUser> manager, IBlogRepository repository, ILogger<SearchController> logger)
        {
            this._repository = repository;
            this._manager = manager;
            this._logger = logger;
        }


        [HttpGet("{tag}")]

        public async Task<IActionResult> SearchPosts([FromRoute] string tag)
        {
            _logger.LogInformation("Handling GET search posts");
            try
            {
                var posts = await _repository.SearchPostByTag(tag);
                return Ok(posts); // This will return a 200 OK status with the post data
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching posts");
                throw;
            }

                //var postIndexViewModel = new PostIndexViewModel
                //{
                //    Posts = posts,
                //    BlogId = id,
                //    BlogTitle = blog.Title,
                //    IsPostAllowed = blog.IsPostAllowed,
                //};

                //return postIndexViewModel;


        }



    }

}
