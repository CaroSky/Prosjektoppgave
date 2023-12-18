using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SharedModels.ViewModels;
using System.Security.Claims;
using WebAPI.Models.Repositories;
using SharedModels.Entities;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;
using WebAPI.Hubs;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : Controller
    {
        private IBlogRepository _repository;
        private UserManager<IdentityUser> _manager;
        private readonly ILogger<PostController> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        public PostController(UserManager<IdentityUser> manager, IBlogRepository repository, ILogger<PostController> logger, IHubContext<NotificationHub> hubContext)
        {
            _repository = repository;
            _manager = manager;
            _logger = logger;
            _hubContext = hubContext;
        }


        [HttpGet("{id}/posts")]

        public async Task<PostIndexViewModel> GetPosts([FromRoute] int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);   //it return a http://...:username so I need to get the username from the string

            if (userIdClaim != null)
            {
                var userId = userIdClaim.Value;
                _logger.LogInformation($"User ID in blog Controller - GetBlogs: {userId}");
                string[] words = userIdClaim.ToString().Split(':');
                string username = words[words.Length - 1].Trim();
                var user = await _manager.FindByNameAsync(username);
                _logger.LogInformation($"Hentet bruker: {user?.UserName}, ID: {user?.Id}");
            }
            else
            {
                _logger.LogWarning("User ID claim not found.");
            }

            var blog = await _repository.GetBlogById(id);
            var posts = await _repository.GetAllPostByBlogId(id);
            var postIndexViewModel = new PostIndexViewModel
            {
                Posts = posts,
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
                   // Author = await _manager.FindByNameAsync(User.Identity.Name),
                    Blog = await _repository.GetBlogById(postCreateViewModel.BlogId),
                    IsCommentAllowed = true,
                    OwnerId = user.Id,
                };
            
            // Ekstraher tags fra innholdet
            var tags = ExtractHashtags(post.Content);
            var userNames = ExtractUsernames(post.Content);
            _logger.LogInformation($"Extracted hashtags: {string.Join(", ", tags)}");
            _logger.LogInformation($"Extracted usernames: {string.Join(", ", userNames)}");
            //Lagrer post
            await _repository.SavePost(post, User);
            _logger.LogInformation("Post saved successfully.");
            //tempdata
            //TempData["message"] = string.Format("{0} has been created", post.Title);

            var allTags = tags.Concat(userNames).ToList();

            foreach (var tagName in allTags)
            {
                var tag = await _repository.GetTagByName(tagName) ?? new Tag { Name = tagName };

                if (tag.TagId == 0)
                {
                    await _repository.SaveTag(tag);
                    _logger.LogInformation($"New tag created: {tagName}");
                }

                var postTag = new PostTag { PostsPostId = post.PostId, TagsTagId = tag.TagId };
                await _repository.SavePostTag(postTag);
                _logger.LogInformation($"Tag {tagName} associated with the post.");
            }
            foreach (var userName in userNames)
            {
                var taggedUser = await _manager.FindByNameAsync(userName);
                if (taggedUser != null)
                {
                    _logger.LogInformation($"Bruker funnet, sender notifikasjon: {userName}");
                    try
                    {
                        _logger.LogInformation($"Sending tag notification to user: {taggedUser.UserName}");
                        await _hubContext.Clients.User(taggedUser.UserName).SendAsync("ReceiveTagNotification", $"Du har blitt tagget i et innlegg av {user.UserName}.");
                        _logger.LogInformation($"Notifikasjon sendt til bruker: {userName}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Feil ved sending av notifikasjon til bruker: {userName}");
                    }
                }
                else
                {
                    _logger.LogWarning($"Fant ingen bruker med brukernavn: {userName}");
                }
            }
            _logger.LogInformation("Post creation process completed.");
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

        private List<string> ExtractUsernames(string content)
        { //Oppdatert slik at den tar emailadresser også som username
            var userNames = Regex.Matches(content, @"@\w+([.-]\w+)*@\w+([.-]\w+)*")
                                  .Cast<Match>()
                                  .Select(match => match.Value.TrimStart('@'))
                                  .ToList();
            _logger.LogInformation("Extracted Usernames: {Usernames}", string.Join(", ", userNames));
            return userNames;
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
            };
            //find the owner (the person logged in)
           // post.Author = await _manager.FindByNameAsync(User.Identity.Name);

            
            await _repository.RemovePostTags(post.PostId);
            // Fjern foreldreløse tags
            await _repository.RemoveOrphanedTags();

            var tags = ExtractHashtags(post.Content);
            var userNames = ExtractUsernames(post.Content);
            await _repository.UpdatePost(post, User);

            var allTags = tags.Concat(userNames).ToList();


            foreach (var tagName in allTags)
            {
                var tag = await _repository.GetTagByName(tagName) ?? new Tag { Name = tagName };
                if (tag.TagId == 0)
                {
                    await _repository.SaveTag(tag);
                }
                var postTag = new PostTag { PostsPostId = post.PostId, TagsTagId = tag.TagId };
                await _repository.SavePostTag(postTag);
            }
            foreach (var userName in userNames)
            {
                var taggedUser = await _manager.FindByNameAsync(userName);
                if (taggedUser != null)
                {
                    _logger.LogInformation($"Bruker funnet, sender notifikasjon: {userName}");
                    try
                    {
                        _logger.LogInformation($"Sending tag notification to user: {taggedUser.UserName}");
                        await _hubContext.Clients.User(taggedUser.UserName).SendAsync("ReceiveTagNotification", $"Du har blitt tagget i et innlegg av {user.UserName}.");
                        _logger.LogInformation($"Notifikasjon sendt til bruker: {userName}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Feil ved sending av notifikasjon til bruker: {userName}");
                    }
                }
                else
                {
                    _logger.LogWarning($"Fant ingen bruker med brukernavn: {userName}");
                }
            }
            _logger.LogInformation("Post creation process completed.");
           // return CreatedAtAction("Get", new { id = post.PostId }, post);

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
