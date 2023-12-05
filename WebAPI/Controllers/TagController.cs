using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Repositories;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagController : Controller

    {
        private readonly IBlogRepository _repository;
        private readonly UserManager<IdentityUser> _manager;

        public TagController(UserManager<IdentityUser> manager, IBlogRepository repository)
        {
            this._repository = repository;
            this._manager = manager;
        }
    }
}
