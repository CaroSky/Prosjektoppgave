using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SharedModels.ViewModels;
using System.Reflection.Metadata;
using System.Security.Claims;
//using WebAPI.Models.Entities;
using WebAPI.Models.Repositories;
using WebAPI.Models.ViewModels;
using SharedModels.Entities;
using System.Text.RegularExpressions;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : Controller
    {
        private IBlogRepository _repository;

        private UserManager<IdentityUser> _manager;
        private readonly ILogger<PostController> _logger;

        private string _username = "til061@uit.no";



        public PostController(UserManager<IdentityUser> manager, IBlogRepository repository, ILogger<PostController> logger)
        {
            this._repository = repository;
            this._manager = manager;
            _logger = logger;
        }


        [HttpGet("{id}/posts")]
        public async Task<ActionResult<PostIndexViewModel>> GetPosts([FromRoute] int id)
        {
            string userId = null;

            // Extract the username from the claim and find the user
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null)
            {
                var claimValue = userIdClaim.Value;
                _logger.LogInformation($"Claim value in blog Controller - GetBlogs: {claimValue}");
                string[] words = claimValue.Split(':');
                string username = words[words.Length - 1].Trim();

                var user = await _manager.FindByNameAsync(username);
                if (user != null)
                {
                    userId = user.Id; // Use the user's ID for like checking
                }
            }
            else
            {
                _logger.LogWarning("User ID claim not found.");
                // Optionally, handle unauthenticated scenario here
            }

            var blog = await _repository.GetBlogById(id);
            var posts = await _repository.GetAllPostByBlogId(id);

            var userLikedPosts = new Dictionary<int, bool>();
            foreach (var post in posts)
            {
                bool isLiked = false;
                if (!string.IsNullOrEmpty(userId))
                {
                    isLiked = await _repository.CheckIfUserLikedPost(post.PostId, userId);
                    _logger.LogInformation($"Post ID: {post.PostId}, Liked by User ID: {userId} = {isLiked}");
                }
                userLikedPosts[post.PostId] = isLiked;
            }

            var postIndexViewModel = new PostIndexViewModel
            {
                Posts = posts,
                BlogId = id,
                BlogTitle = blog.Title,
                IsPostAllowed = blog.IsPostAllowed,
                UserLiked = userLikedPosts
            };

            return postIndexViewModel;
        }


        //POST: Create
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody] PostCreateViewModel postCreateViewModel)
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

            //Kall til metoden save i repository
            var post = new Post
            {
                Title = postCreateViewModel.Title,
                Content = postCreateViewModel.Content,
                Created = DateTime.Now,
                Blog = await _repository.GetBlogById(postCreateViewModel.BlogId),
                IsCommentAllowed = postCreateViewModel.IsCommentAllowed,
                OwnerId = user.Id,
                ImageBase64 = postCreateViewModel.ImageBase64 // Include image base64 data
            };

            // Ekstraher tags fra innholdet
            var tags = ExtractHashtags(post.Content);
            //Lagrer post
            await _repository.SavePost(post, User);
            //tempdata
            //TempData["message"] = string.Format("{0} has been created", post.Title);

            foreach (var tagName in tags)
            {
                // Sjekk om taggen finnes, hvis ikke opprett ny
                var tag = await _repository.GetTagByName(tagName) ?? new Tag { Name = tagName };



                // Lagre taggen om den er ny
                if (tag.TagId == 0)
                {
                    await _repository.SaveTag(tag);
                }

                // Opprett og lagre PostTag-forbindelse
                var postTag = new PostTag { PostsPostId = post.PostId, TagsTagId = tag.TagId };
                await _repository.SavePostTag(postTag);
            }
            return CreatedAtAction("Get", new { id = post.PostId }, post);


        }

        private List<string> ExtractHashtags(string content)
        {
            var tags = Regex.Matches(content, @"#\w+")
                        .Cast<Match>()
                        .Select(match => match.Value.TrimStart('#'))
                        .ToList();
            _logger.LogInformation("Extracted Tags: {Tags}", string.Join(", ", tags));
            return tags;
        }


        // GET: Edit
        [HttpGet("{id}")]
        //[Authorize]
        public async Task<IActionResult> Get(int id, int blogId)
        //blogId is used to get back to the blog page if post not found
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

            //---------------------------------------------------------
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //find post
            var post = await _repository.GetPostById(id);

            if (post == null)
            {
                return NotFound();
            }

            var postEdit = await _repository.GetPostEditViewModelById(id);

           // var currentUser = await _manager.FindByNameAsync(User.Identity.Name);
            //if (currentUser.Id == post.Author.Id)
            //{
               return Ok(postEdit);
           // }
            //TempData["message"] = "You cannot edit this item";
           // return BadRequest(ModelState);

        }

        //PUT: Edit
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] PostEditViewModel postEditViewModel)
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

            await _repository.RemovePostTags(id);


            if (id != postEditViewModel.PostId)
            {
                return BadRequest();
            }

            var post = new Post
            {
                PostId = postEditViewModel.PostId,
                Title = postEditViewModel.Title,
                Content = postEditViewModel.Content,
                Created = postEditViewModel.Created,
                IsCommentAllowed = postEditViewModel.IsCommentAllowed,
                Blog = await _repository.GetBlogById(postEditViewModel.BlogId),
                OwnerId = user.Id,
                ImageBase64 = postEditViewModel.ImageBase64 // Update image base64 data
            };
            //find the owner (the person logged in)
            // post.Author = await _manager.FindByNameAsync(User.Identity.Name);

            await _repository.UpdatePost(post, User);
            // _repository.Update(product);
            //tempdata
            //TempData["message"] = string.Format("{0} has been updated", post.Title);

            var tags = ExtractHashtags(post.Content);
            foreach (var tagName in tags)
            {
                var tag = await _repository.GetTagByName(tagName) ?? new Tag { Name = tagName };
                if (tag.TagId == 0)
                {
                    await _repository.SaveTag(tag);
                }
                var postTag = new PostTag { PostsPostId = post.PostId, TagsTagId = tag.TagId };
                await _repository.SavePostTag(postTag);
            }

            // Fjern foreldreløse tags
            await _repository.RemoveOrphanedTags();


            return Ok(post);

        }


        // GET: Delete
        [HttpDelete("{id}")]
        //[Authorize]
        public async Task<IActionResult> Delete([FromRoute] int id, int blogId)
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
            var post = await _repository.GetPostById(id);

            if (post == null)
            {
                return NotFound();
            }

             await _repository.DeletePost(post, User);
            //await _repository.RemoveOrphanedTags();
            return Ok(post);
        }
        // POST: Like a Post
        [HttpPost("{postId}/like")]
        [Authorize]
        public async Task<IActionResult> LikePost([FromRoute] int postId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            string[] words = userIdClaim.ToString().Split(':');
            string username = words[words.Length - 1].Trim();
            var user = await _manager.FindByNameAsync(username);
            _logger.LogInformation($"Inside LikePost with postid {postId} and userId {user.Id}");

            await _repository.ToggleLikePost(postId, user.Id);

            return Ok();
        }

    }
}
