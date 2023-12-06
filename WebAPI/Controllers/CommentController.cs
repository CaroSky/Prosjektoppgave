using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using WebAPI.Models.Entities;
using WebAPI.Models.Repositories;
using WebAPI.Models.ViewModels;
using System.Reflection.Metadata;
using System.Security.Claims;
using SharedModels.Entities;
using SharedModels.ViewModels;
using Microsoft.Extensions.Logging;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : Controller
    {
        private IBlogRepository _repository;

        private UserManager<IdentityUser> _manager;

        private readonly ILogger<CommentController> _logger;


        public CommentController(UserManager<IdentityUser> manager, IBlogRepository repository, ILogger<CommentController> logger)
        {
            this._repository = repository;
            this._manager = manager;
            _logger = logger;
        }

        [HttpGet("{id}/comments")]
        public async Task<CommentIndexViewModel> GetComments([FromRoute] int id)
        {

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


                _logger.LogInformation("Henter kommentarer for post med ID: {PostId}", id);
                var comments = await _repository.GetAllCommentsByPostId(id);
                _logger.LogInformation("Antall hentede kommentarer: {Count}", comments.Count());


                var post = await _repository.GetPostById(id);

                if (post == null)
                {
                    _logger.LogWarning("Ingen post funnet med ID: {PostId}", id);
                   
                }

                var commentIndexViewModel = new CommentIndexViewModel
                {
                    Comments = comments,
                    PostId = id,
                    BlogId = post.Blog.BlogId,
                    PostTitle = post.Title,
                    IsCommentAllowed = post.IsCommentAllowed,
                };

                return commentIndexViewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "En feil oppstod ved henting av kommentarer for post med ID: {PostId}", id);
                throw;
            }
        }


        //POST: Create
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CommentCreateViewModel commentCreateViewModel)
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



            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Kall til metoden save i repository
            var comment = new Comment()
            {
                Content = commentCreateViewModel.Content,
                Created = DateTime.Now,
                //Author = await _manager.FindByNameAsync(User.Identity.Name),
                Post = await _repository.GetPostById(commentCreateViewModel.PostId),
                OwnerId = user.Id,
            };


            await _repository.SaveComment(comment, User);

            return CreatedAtAction("Get", new { id = comment.CommentId }, comment);
            
        }


        // GET: Edit
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id, int postId)
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



            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //sjekk om id finnes i product list
            var comment = await _repository.GetCommentById(id);
            //var id = comment.Post.PostId;
            if (comment == null)
            {
                return NotFound();
            }

            var commentEdit = await _repository.GetCommentEditViewModelById(id);


            var currentUser = await _manager.FindByNameAsync(User.Identity.Name);
            //if (currentUser.Id == comment.Author.Id)
            {
                return Ok(commentEdit);
            }
            TempData["message"] = "You cannot edit this item";
            return BadRequest(ModelState);


        }

        //PUT: Edit
       [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] CommentEditViewModel commentEditViewModel)
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



            if (!ModelState.IsValid)
                
            {
                _logger.LogWarning("Modellen er ikke gyldig for kommentar med ID {id}");
                return BadRequest(ModelState);
            }

            if (id != commentEditViewModel.CommentId)
            {
                 return BadRequest();
            }

            var comment = new Comment()
            {
                CommentId = commentEditViewModel.CommentId,
                Content = commentEditViewModel.Content,
                Created = commentEditViewModel.Created,
                Post = await _repository.GetPostById(commentEditViewModel.PostId),
                OwnerId = user.Id,
            };
            //find the owner (the person logged in)
            // comment.Author = await _manager.FindByNameAsync(User.Identity.Name);

            _logger.LogInformation("Oppdaterer kommentar med ID {id} i databasen");
            await _repository.UpdateComment(comment, User);
            _logger.LogInformation("Kommentar med ID {id} er oppdatert i databasen");

             //_repository.UpdateComment(product);


            return Ok(comment);
        }


        // GET: Delete
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] int id, int postId)
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


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var comment = await _repository.GetCommentById(id);

            if (comment == null)
            {
                return NotFound();
            }

           // var currentUser = await _manager.FindByNameAsync(User.Identity.Name);
            //if (currentUser.Id == comment.Author.Id)
            {
                await _repository.DeleteComment(comment, User);
                //tempdata
                //TempData["message"] = string.Format("The comment has been deleted");
                return Ok(comment);
            }
            //else
            //{
              //  return BadRequest(ModelState);
            //}


        }
    }
}