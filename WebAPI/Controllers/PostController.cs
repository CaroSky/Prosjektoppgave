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
using System.Collections.Generic;
using System.Linq;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : Controller
    {
        private IBlogRepository _repository;

        private UserManager<IdentityUser> _manager;
        private readonly ILogger<PostController> _logger;




        public PostController(UserManager<IdentityUser> manager, IBlogRepository repository, ILogger<PostController> logger)
        {
            this._repository = repository;
            this._manager = manager;
            _logger = logger;
        }


        [HttpGet("{id}/posts")]

        public async Task<PostIndexViewModel> GetPosts([FromRoute] int id)
        {
            IdentityUser user = null;

            //find the user that is logged in 
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);   //it return a http://...:username so I need to get the username from the string
            if (userIdClaim != null)
            {
                var userId = userIdClaim.Value;
                _logger.LogInformation($"User ID in blog Controller - GetBlogs: {userId}");
                string[] words = userIdClaim.ToString().Split(':');
                string username = words[words.Length - 1].Trim();
                user = await _manager.FindByNameAsync(username);


            }
            else
            {
                _logger.LogWarning("User ID claim not found.");
            }


            var blog = await _repository.GetBlogById(id);
            var posts = await _repository.GetAllPostByBlogId(id);
            List<PostWithLike> postsWithLikeList = new List<PostWithLike>();

            foreach (var post in posts)
            {
                var newPost = new PostWithLike()
                {
                    Post = post,
                    Like = ""
                };
                postsWithLikeList.Add(newPost);
            }

            if (user != null)
            {
                var likes = await _repository.GetAllLikesForUser(user.Id);
                // Use LINQ to create a list of just the PostIds
                List<int> postIdLikeList = likes.Select(post => post.PostId).ToList();

                foreach (var post in postsWithLikeList)
                {
                    if (postIdLikeList.Contains(post.Post.PostId))
                    {
                        post.Like = "Liked!";

                    }

                }
            }


            // Convert the list to IEnumerable<PostWithLike>
            IEnumerable<PostWithLike> postsWithLike = postsWithLikeList;
            var postIndexViewModel = new PostIndexViewModel
            {
                Posts = postsWithLike,
                BlogId = id,
                BlogTitle = blog.Title,
                IsPostAllowed = blog.IsPostAllowed,
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
                    IsCommentAllowed = true,
                    OwnerId = user.Id,
                    OwnerUsername = user.UserName,
                    CountLike = 0,
                    ImageBase64 = ""
                };
            
            // Ekstraher tags fra innholdet
            var tags = ExtractHashtags(post.Content);
            //Lagrer post
            await _repository.SavePost(post, User);


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


            var subscribeUserList = await _repository.GetSubscriptionsByBlogId(post.Blog.BlogId);
            var notifications = new List<Notfication>();

            if (post.OwnerId != post.Blog.OwnerId)
            {
                var newNotification = new Notfication()
                {
                    PostId = post.PostId,
                    UserId = post.Blog.OwnerId
                };
                notifications.Add(newNotification);
            }

            if (subscribeUserList != null)
            {
                foreach (var subscription in subscribeUserList)
                {
                    var newNotification = new Notfication()
                    {
                        PostId = post.PostId,
                        UserId = subscription.UserId
                    };
                    notifications.Add(newNotification);
                }

                
            }

            if (notifications.Count > 0)
            {
                await _repository.InsertMultipleNotifications(notifications);
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
        public async Task<PostIndexViewModel> Get(int id, int blogId)
        //blogId is used to get back to the blog page if post not found
        {
            IdentityUser user = null;
            //find the user that is logged in 
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);   //it return a http://...:username so I need to get the username from the string
            if (userIdClaim != null)
            {
                var userId = userIdClaim.Value;
                _logger.LogInformation($"User ID in blog Controller - GetBlogs: {userId}");
                string[] words = userIdClaim.ToString().Split(':');
                string username = words[words.Length - 1].Trim();
                user = await _manager.FindByNameAsync(username);
            }
            else
            {
                _logger.LogWarning("User ID claim not found.");
            }

            //---------------------------------------------------------
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("The model is not valid.");
            }

            //find post
            var post = await _repository.GetPostById(id);

            if (post == null)
            {
                _logger.LogWarning("No post found."); ;
            }
            List<PostWithLike> postsWithLikeList = new List<PostWithLike>();

            var postWithLike = new PostWithLike()
            {
                Post = post,
                Like = ""
            };

            if (user != null)
            {
                var likes = await _repository.GetAllLikesForUser(user.Id);
                // Use LINQ to create a list of just the PostIds
                List<int> postIdLikeList = likes.Select(post => post.PostId).ToList();

                if (postIdLikeList.Contains(postWithLike.Post.PostId))
                {
                    postWithLike.Like = "Liked!";

                }

            }

            var posts = new List<PostWithLike>();
            posts.Add(postWithLike);
            var postIndexViewModel = new PostIndexViewModel
            {
                Posts = posts,
                BlogId = 0,
                BlogTitle = "",
                IsPostAllowed = false,
            };

            return postIndexViewModel;

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
                OwnerId = postEditViewModel.OwnerId,
                OwnerUsername = postEditViewModel.OwnerUsername,
                CountLike = postEditViewModel.CountLike,
                ImageBase64 = postEditViewModel.ImageBase64,
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
    }
}
