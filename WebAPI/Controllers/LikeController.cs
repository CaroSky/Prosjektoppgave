using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using SharedModels.Entities;
using System.Security.Claims;
using WebAPI.Models.Repositories;

namespace WebAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class LikeController : Controller
    {
        private IBlogRepository _repository;
        private UserManager<IdentityUser> _manager;
        private readonly ILogger<LikeController> _logger;


        public LikeController(UserManager<IdentityUser> manager, IBlogRepository repository, ILogger<LikeController> logger)
        {
            this._repository = repository;
            this._manager = manager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetLikes()
        {
            _logger.LogInformation("Handling GET request for likes");
            try
            {
                //find the user that is logged in 
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);   //it return a http://...:username so I need to get the username from the string

                if (userIdClaim != null)
                {
                    var userId = userIdClaim.Value;
                    _logger.LogInformation($"User ID in like Controller - GetLikes: {userId}");
                    string[] words = userIdClaim.ToString().Split(':');
                    string username = words[words.Length - 1].Trim();
                    var user = await _manager.FindByNameAsync(username);
                }
                else
                {
                    _logger.LogWarning("User ID claim not found.");
                }

                var likes = await _repository.GetAllLikes();

                _logger.LogInformation($"Username: {User.FindFirst(ClaimTypes.Name)?.Value}");
                return Ok(likes); // This will return a 200 OK status with the blogs data
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching likes");
                throw;
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody] int postId)
        {
            //find the user that is logged in 
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);   //it return a http://...:username so I need to get the username from the string
            string[] words = userIdClaim.ToString().Split(':');
            string username = words[words.Length - 1].Trim();
            var user = await _manager.FindByNameAsync(username);

            if (user == null)
            {
                return Unauthorized(); // Brukeren er ikke autentisert
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var like = new Like()
            {
                PostId = postId,
                UserId = user.Id
            };

            await _repository.SaveLike(like);
            return Ok(like);
        }

        // GET: Product/Delete
        [HttpDelete("{postid}")]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] int postid)
        {
            //find the user that is logged in 
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);   //it return a http://...:username so I need to get the username from the string
            string[] words = userIdClaim.ToString().Split(':');
            string username = words[words.Length - 1].Trim();
            var user = await _manager.FindByNameAsync(username);

            if (user == null)
            {
                return Unauthorized(); // Brukeren er ikke autentisert
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var like = await _repository.GetLike(postid, user.Id);

            if (like == null)
            {
                return NotFound();
            }

            await _repository.DeleteLike(like);

            return Ok(like);

        }

    }
}
