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

        private string _username = "cla040@uit.no";



        public BlogController(UserManager<IdentityUser> manager, IBlogRepository repository)
        {
            this._repository = repository;
            this._manager = manager;
        }


        [HttpGet]
        public async Task<IEnumerable<Blog>> GetBlogs()
        {
            return await _repository.GetAllBlogs();
        }



        // GET: Product/Create
        //[HttpGet]
        //[Authorize]
        //public ActionResult Create()
        //{
        //    var blog = _repository.GetBlogCreateViewModel();
        //
        //    return View(blog);
        //}

        //POST: Product/Create
        [HttpPost]
        //[Authorize]
        public async Task<IActionResult> Post([FromBody] BlogCreateViewModel blogCreateViewModel)
        {
            //TOVE: to be removed, har fjernet claims:ASP.NET Core Identity håndterer brukerprinsipper for deg.
            //var user = await _manager.FindByNameAsync(_username);
            var userId =  _manager.GetUserId(User);
            /*if (userId != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userId.UserName),
                    // Add other claims as needed
                };
                var identity = new ClaimsIdentity(claims, "custom");
                var principal = new ClaimsPrincipal(identity);

                // Set the principal to HttpContext.User
                HttpContext.User = principal;
            }*/
            //---------------------------------------------------------
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(); // Brukeren er ikke autentisert
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            //Kall til metoden save i repository
            var blog = new Blog
            {
                Title = blogCreateViewModel.Title,
                Content = blogCreateViewModel.Content,
                Created = DateTime.Now,
                UserId = userId, // Sett den autentiserte brukerens ID
                //Owner = await _manager.FindByNameAsync(User.Identity.Name),
                IsPostAllowed = true
            };


            await _repository.SaveBlog(blog, User);
            //tempdata
            //TempData["message"] = string.Format("{0} has been created", blog.Title);

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
            if (userId == blog.UserId)
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
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] BlogEditViewModel blogEditViewModel)
        {
            // Få den innloggede brukerens ID
            var userId = _manager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }
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

            if (id != blogEditViewModel.BlogId)
            {
                return BadRequest();
            }


            //Kall til metoden save i repository

            var blog = new Blog
                {
                    BlogId = blogEditViewModel.BlogId,
                    Title = blogEditViewModel.Title,
                    Content = blogEditViewModel.Content,
                    Created = blogEditViewModel.Created,
                    IsPostAllowed = blogEditViewModel.IsPostAllowed
                };
            //find the owner (the person logged in)
            //TOVE. Gjernet denne:
            //blog.Owner = await _manager.FindByNameAsync(User.Identity.Name);

            await _repository.UpdateBlog(blog, User);
            // _repository.Update(product);
            //tempdata
            //TempData["message"] = string.Format("{0} has been updated", blog.Title);

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

            //tempdata
            //TempData["message"] = string.Format("{0} has been deleted", blog.Title);
           
            return Ok(blog);

        }


    }
}