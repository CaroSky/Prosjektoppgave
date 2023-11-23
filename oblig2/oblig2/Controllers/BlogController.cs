using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using oblig2.Models.Entities;
using oblig2.Models.Repositories;
using oblig2.Models.ViewModels;
using System.Xml.Linq;

namespace oblig2.Controllers
{
    public class BlogController : Controller
    {
        private IBlogRepository _repository;

        private UserManager<IdentityUser> _manager;

        private IAuthorizationService _authorizationService;



        public BlogController(UserManager<IdentityUser> manager, IBlogRepository repository)
        {
            this._repository = repository;
            this._manager = manager;
        }
        public IActionResult Index()
        {
            var blogs = _repository.GetAllBlogs();
            return View(blogs);
        }



        // GET: Product/Create
        [Authorize]
        public ActionResult Create()
        {
            var blog = _repository.GetBlogCreateViewModel();

            return View(blog);
        }

        //POST: Product/Create
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Title,Content")] BlogCreateViewModel blogCreateViewModel)
        {

            if (ModelState.IsValid)
            {
                //Kall til metoden save i repository
                var blog = new Blog
                {
                    Title = blogCreateViewModel.Title,
                    Content = blogCreateViewModel.Content,
                    Created = DateTime.Now,
                    Owner = await _manager.FindByNameAsync(User.Identity.Name),
                    IsPostAllowed = true
                };


                await _repository.SaveBlog(blog, User);
                //tempdata
                TempData["message"] = string.Format("{0} has been created", blog.Title);

                return RedirectToAction(("Index"));
            }

            return View(blogCreateViewModel);

        }


        // GET: Product/Edit
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {


            //sjekk om id finnes i product list
            List<int> idList = _repository.GetAllBlogs().Select(item => item.BlogId).Distinct().ToList();
            if (idList.Contains(id))
            {
                //find product
                var blog = _repository.GetBlogById(id);

                var blogEdit = _repository.GetBlogEditViewModelById(id);

                var currentUser = await _manager.FindByNameAsync(User.Identity.Name);
                if (currentUser.UserName == blog.Owner.UserName)
                {
                    return View(blogEdit);
                }

                TempData["message"] = "You cannot edit this item";
                return RedirectToAction(("Index"));

                
            }
            //tempdata
            TempData["message"] = "Item not found";
            return RedirectToAction(("Index"));

        }

        //POST: Product/Edit
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit([Bind("BlogId,Title,Content,Created,IsPostAllowed")] BlogEditViewModel blogEditViewModel)
        {
            if (ModelState.IsValid)
            {
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
                blog.Owner = await _manager.FindByNameAsync(User.Identity.Name);

                await _repository.UpdateBlog(blog, User);
                // _repository.Update(product);
                //tempdata
                TempData["message"] = string.Format("{0} has been updated", blog.Title);

                return RedirectToAction(("Index"));
            }

            return View(blogEditViewModel);

        }


        // GET: Product/Delete
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            //sjekk om id finnes i product list
            List<int> idList = _repository.GetAllBlogs().Select(item => item.BlogId).Distinct().ToList();
            if (idList.Contains(id))
            {
                var blog = _repository.GetBlogById(id);

                await _repository.DeleteBlog(blog, User);
                
                //tempdata
                TempData["message"] = string.Format("{0} has been deleted", blog.Title);
            }
            else
            {
                //tempdata
                TempData["message"] = "Item not found";
            }

            return RedirectToAction(("Index"));

        }


    }
}
