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
    //[Route("api/blog")]
    public class BlogController : ControllerBase
    {
        private IBlogRepository _repository;

        private UserManager<IdentityUser> _manager;

        private IAuthorizationService _authorizationService;

        private string _username = "testuser@example.com";
        private readonly ILogger<BlogController> _logger;



        public BlogController(UserManager<IdentityUser> manager, IBlogRepository repository, ILogger<BlogController> logger)
        {
            this._repository = repository;
            this._manager = manager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetBlogs()
        {
            _logger.LogInformation("Handling GET request for blogs");
            try
            {
                var blogs = await _repository.GetAllBlogs();
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
        //[Authorize]
        //public async Task<IActionResult> Post([FromBody] BlogCreateViewModel blogCreateViewModel)
        public async Task<IActionResult> Post([FromBody] Blog blog)
        {
            //TOVE: to be removed, har fjernet claims:ASP.NET Core Identity håndterer brukerprinsipper for deg.
            var user = await _manager.FindByNameAsync(_username);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    // Add other claims as needed
                };
                var identity = new ClaimsIdentity(claims, "custom");
                var principal = new ClaimsPrincipal(identity);

                // Set the principal to HttpContext.User
                HttpContext.User = principal;
            }
            //var userId = _manager.GetUserId(User);
            //---------------------------------------------------------
            //if (string.IsNullOrEmpty(userI))
            if (user == null)
            {
                return Unauthorized(); // Brukeren er ikke autentisert
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
 
            blog.OwnerId = user.Id;

            await _repository.SaveBlog(blog, User);

            return CreatedAtAction("Get", new { id = blog.BlogId }, blog);


        }


        // GET: Product/Edit
        [HttpGet("{id}")]
        //[Authorize]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            //to be removed
            var user = await _manager.FindByNameAsync(_username);
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    // Add other claims as needed
                };
                var identity = new ClaimsIdentity(claims, "custom");
                var principal = new ClaimsPrincipal(identity);

                // Set the principal to HttpContext.User
                HttpContext.User = principal;
            }
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

            //var currentUser = await _manager.FindByNameAsync(User.Identity.Name);
            //if (currentUser.UserName == blog.Owner.UserName)
            // Hent den innloggede brukerens ID
            var userId = _manager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            // Sjekk om den innloggede brukeren er eieren av bloggposten
            if (userId == blog.OwnerId)
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
        //[Authorize]
        //public async Task<IActionResult> Put([FromRoute] int id, [FromBody] BlogEditViewModel blogEditViewModel)
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Blog blog)
        {
            // Få den innloggede brukerens ID
            //var userId = _manager.GetUserId(User);
            //if (userId == null)
            //{
            //    return Unauthorized();
            //}
            //to be removed
            /*// var user = await _manager.FindByNameAsync(_username);
             if (user != null)
             {
                 var claims = new List<Claim>
                 {
                     new Claim(ClaimTypes.Name, user.UserName),
                     // Add other claims as needed
                 };
                 var identity = new ClaimsIdentity(claims, "custom");
                 var principal = new ClaimsPrincipal(identity);

                 // Set the principal to HttpContext.User
                 HttpContext.User = principal;
             }*/
            //---------------------------------------------------------
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //if (id != blogEditViewModel.BlogId)
            //{
            //    return BadRequest();
            //}

       
            //find the owner (the person logged in)
            //TOVE. Gjernet denne:
            //blog.Owner = await _manager.FindByNameAsync(User.Identity.Name);

            await _repository.UpdateBlog(blog);

            return Ok(blog);

        }


        // GET: Product/Delete
        [HttpDelete("{id}")]
        //[Authorize]
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


    }
}