using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using WebAPI.Models.Repositories;
using SharedModels.Entities;
using SharedModels.ViewModels;
using Microsoft.Extensions.Hosting;


namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : Controller
    {
        private IBlogRepository _repository;
        private UserManager<IdentityUser> _manager;
        private readonly ILogger<NotificationController> _logger;


        public NotificationController(UserManager<IdentityUser> manager, IBlogRepository repository,
            ILogger<NotificationController> logger)
        {
            this._repository = repository;
            this._manager = manager;
            _logger = logger;
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetNotificationPosts()
        {
            _logger.LogInformation("Handling GET request for notifications");
            try
            {
                var postLit = new List<Post>();
                var postWithLikeList = new List<PostWithLike>();
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);   //it return a http://...:username so I need to get the username from the string
                string[] words = userIdClaim.ToString().Split(':');
                string username = words[words.Length - 1].Trim();
                var user = await _manager.FindByNameAsync(username);
                if (user != null)
                    {
                        var notifications = await _repository.GetAllNotificationsForUser(user.Id);
                        _logger.LogInformation($"Username: {User.FindFirst(ClaimTypes.Name)?.Value}");
                        if (notifications != null)
                        {
                            foreach (var notification in notifications)
                            {
                                var post = await _repository.GetPostById(notification.PostId);
                                postLit.Add(post);
                            }
                            var likes = await _repository.GetAllLikesForUser(user.Id);
                            // Use LINQ to create a list of just the PostIds
                            List<int> postIdLikeList = likes.Select(post => post.PostId).ToList();

                            foreach (var post in postLit)
                            {
                                var newPostWithLike = new PostWithLike()
                                {
                                    Post = post,
                                    Like = ""
                                };
                                if (postIdLikeList.Contains(post.PostId))
                                {
                                    newPostWithLike.Like = "Liked!";

                                }
                                postWithLikeList.Add(newPostWithLike);

                            }
                        }
                    }


                return Ok(postLit); // This will return a 200 OK status with the blogs data
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching likes");
                throw;
            }
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetNotificationsCount()
        {
            _logger.LogInformation("Handling GET request for notifications count");
            try
            {
                var count = 0;
                //find the user that is logged in 
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier); //it return a http://...:username so I need to get the username from the string
                if (userIdClaim != null)
                {
                    var userId = userIdClaim.Value;
                    _logger.LogInformation($"User ID in blog Controller - GetBlogs: {userId}");
                    string[] words = userIdClaim.ToString().Split(':');
                    string username = words[words.Length - 1].Trim();
                    var user = await _manager.FindByNameAsync(username);
                    if (user != null)
                    {
                        count = await _repository.GetNotificationsCountForUser(user.Id);
                    }

                    _logger.LogInformation($"Username: {User.FindFirst(ClaimTypes.Name)?.Value}");
                }

                return Ok(count); // This will return a 200 OK status with the blogs data
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching notification count");
                throw;
            }
        }

        // GET: Delete
        [HttpDelete("{id}")]
        //[Authorize]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            //find the user that is logged in 
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);   //it return a http://...:username so I need to get the username from the string
            if (userIdClaim == null)
            {
                return Unauthorized(); // Brukeren er ikke autentisert
            }
            string[] words = userIdClaim.ToString().Split(':');
            string username = words[words.Length - 1].Trim();
            var user = await _manager.FindByNameAsync(username);


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var notification = await _repository.GetNotification(id, user.Id);

            if (notification == null)
            {
                return NotFound();
            }

            await _repository.DeleteNotification(notification);
            //await _repository.RemoveOrphanedTags();
            return Ok(notification);
        }

        // GET: Delete
        [HttpDelete]
        //[Authorize]
        public async Task<IActionResult> Delete()
        {
            //find the user that is logged in 
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);   //it return a http://...:username so I need to get the username from the string
            if (userIdClaim == null)
            {
                return Unauthorized(); // Brukeren er ikke autentisert
            }
            string[] words = userIdClaim.ToString().Split(':');
            string username = words[words.Length - 1].Trim();
            var user = await _manager.FindByNameAsync(username);
            if (user == null)
            {
                return Unauthorized(); // Brukeren er ikke autentisert
            }

            await _repository.DeleteAllNotificationsForUser(user.Id);

  
            return Ok();


        }
    }
}
