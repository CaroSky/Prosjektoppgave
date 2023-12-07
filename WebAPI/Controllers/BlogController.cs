using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using WebAPI.Models.Entities;
using WebAPI.Models.Repositories;
using WebAPI.Models.ViewModels;
using System.Xml.Linq;
using SharedModels.Entities;



namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class BlogController : ControllerBase
    {
        private IBlogRepository _repository;

        private UserManager<IdentityUser> _manager;
        private SignInManager<IdentityUser> _signManager;

        private IAuthorizationService _authorizationService;

        private readonly ILogger<BlogController> _logger;



        public BlogController(UserManager<IdentityUser> manager, IBlogRepository repository, ILogger<BlogController> logger, SignInManager<IdentityUser> signManager)
        {
            this._repository = repository;
            this._manager = manager;
            _logger = logger;
            _signManager = signManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetBlogs()
        {
            _logger.LogInformation("Handling GET request for blogs");
            try
            {
                //find the user that is logged in 
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);   //it return a http://...:username so I need to get the username from the string

                if (userIdClaim != null)
                {
                    var userId = userIdClaim.Value;
                    _logger.LogInformation($"User ID in blog Controller - GetBlogs: {userId}");
                    string[] words = userIdClaim.ToString().Split(':');
                    string username = words[words.Length - 1].Trim();
                    var user = await _manager.FindByNameAsync(username);
                }
                else
                {
                    _logger.LogWarning("User ID claim not found.");
                }

                var blogs = await _repository.GetAllBlogs();

                _logger.LogInformation($"Username: {User.FindFirst(ClaimTypes.Name)?.Value}");
                return Ok(blogs); // This will return a 200 OK status with the blogs data
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching blogs");
                throw;
            }
        }


        //POST: Product/Create
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody] Blog blog)
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
            
 
            blog.OwnerId = user.Id;
            blog.OwnerUsername = user.UserName;

            await _repository.SaveBlog(blog, User);

            return CreatedAtAction("Get", new { id = blog.BlogId }, blog);


        }


        // GET: Product/Edit
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get([FromRoute] int id)
        {

            //find the user that is logged in 
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);   //it return a http://...:username so I need to get the username from the string
            string[] words = userIdClaim.ToString().Split(':');
            string username = words[words.Length - 1].Trim();
            var user = await _manager.FindByNameAsync(username);

            //---------------------------------------------------------
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //find product
            var blog = await _repository.GetBlogById(id);

            if (blog == null)
            {
                return NotFound();
            }


            var blogEdit = await _repository.GetBlogEditViewModelById(id);


            if (user.Id == null)
            {
                return Unauthorized();
            }

            // Sjekk om den innloggede brukeren er eieren av bloggposten
            if (user.Id == blog.OwnerId)
                    {
                return Ok(blog);
            }
            else
            {
                return BadRequest(ModelState);
                //return Ok(blog);
            }


        }

        //PUT: Product/Edit
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Blog blog)
        {
            //find the user that is logged in 
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);   //it return a http://...:username so I need to get the username from the string
            string[] words = userIdClaim.ToString().Split(':');
            string username = words[words.Length - 1].Trim();
            var user = await _manager.FindByNameAsync(username);

            if (user.Id != blog.OwnerId)
            {
                return Ok("Unauthorized");
            }


            //---------------------------------------------------------
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            await _repository.UpdateBlog(blog);

            return Ok(blog);

        }


        // GET: Product/Delete
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var blog = await _repository.GetBlogById(id);

            if (blog == null)
            {
                return NotFound();
            }

            await _repository.DeleteBlog(blog, User);
           
            return Ok(blog);

        }
        [HttpPost("{blogId}/subscribe")]
        [Authorize]
        public async Task<IActionResult> Subscribe(int blogId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            string[] words = userIdClaim.ToString().Split(':');
            string username = words[words.Length - 1].Trim();
            var user = await _manager.FindByNameAsync(username);
            if (user == null)
            {
                _logger.LogWarning("User ID claim not found.");
                return Unauthorized();
            }

            _logger.LogInformation($"User ID in blog Controller - Subscribe: {user.Id}");

            await _repository.SubscribeToBlog(user.Id, blogId);
            return Ok();
        }

        [HttpPost("{blogId}/unsubscribe")]
        [Authorize]
        public async Task<IActionResult> Unsubscribe(int blogId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            string[] words = userIdClaim.ToString().Split(':');
            string username = words[words.Length - 1].Trim();
            var user = await _manager.FindByNameAsync(username);
            if (user == null)
            {
                _logger.LogWarning("User ID claim not found.");
                return Unauthorized();
            }

            _logger.LogInformation($"User ID in blog Controller - Unsubscribe: {user.Id}");

            await _repository.UnsubscribeFromBlog(user.Id, blogId);
            return Ok();
        }

        [HttpGet("subscriptionStatuses")]
        [Authorize]
        public async Task<IActionResult> GetAllSubscriptionStatuses()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            string[] words = userIdClaim.ToString().Split(':');
            string username = words[words.Length - 1].Trim();
            var user = await _manager.FindByNameAsync(username);
            if (user == null)
            {
                _logger.LogWarning("User ID claim not found.");
                return Unauthorized();
            }

            _logger.LogInformation($"User ID in blog Controller - GetAllSubscriptionStatuses: {user.Id}");

            var subscriptionStatuses = await _repository.GetAllSubscriptionStatuses(user.Id);
            return Ok(subscriptionStatuses);
        }

    }
}