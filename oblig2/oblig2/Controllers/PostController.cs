using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using oblig2.Models.Entities;
using oblig2.Models.Repositories;
using oblig2.Models.ViewModels;

namespace oblig2.Controllers
{
    public class PostController : Controller
    {
        private IBlogRepository _repository;

        private UserManager<IdentityUser> _manager;




        public PostController(UserManager<IdentityUser> manager, IBlogRepository repository)
        {
            this._repository = repository;
            this._manager = manager;
        }
        public IActionResult Index([FromRoute] int id)
        {
            var blog = _repository.GetBlogById(id);
            var posts = _repository.GetAllPostByBlogId(id);
            var postIndexViewModel = new PostIndexViewModel
            {
                Posts = posts,
                BlogId = id,
                BlogTitle = blog.Title,
                IsPostAllowed = blog.IsPostAllowed,
            };
            
            return View(postIndexViewModel);
        }



        // GET: Create
        [Authorize]
        public ActionResult Create(int id)
        {
            var post = _repository.GetPostCreateViewModel(id);

            return View(post);
        }

        //POST: Create
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Title,Content,BlogId")] PostCreateViewModel postCreateViewModel)
        {

            if (ModelState.IsValid)
            {
                //Kall til metoden save i repository
                var post = new Post
                {
                    Title = postCreateViewModel.Title,
                    Content = postCreateViewModel.Content,
                    Created = DateTime.Now,
                    Author = await _manager.FindByNameAsync(User.Identity.Name),
                    Blog = _repository.GetBlogById(postCreateViewModel.BlogId),
                    IsCommentAllowed = true
            };


                await _repository.SavePost(post, User);
                //tempdata
                TempData["message"] = string.Format("{0} has been created", post.Title);

                return RedirectToAction(("Index"), new { id = postCreateViewModel.BlogId });
            }

            return View(postCreateViewModel);

        }


        // GET: Edit
        [Authorize]
        public async Task<IActionResult> Edit(int id, int blogId)
        {


            //sjekk om id finnes i product list
            var post = _repository.GetPostById(id);
            
            if (post == null)
            {
                //tempdata
                TempData["message"] = "Item not found";
                return RedirectToAction(("Index"), new { id = blogId });
            }

            var postEdit = _repository.GetPostEditViewModelById(id);

            var currentUser = await _manager.FindByNameAsync(User.Identity.Name);
            if (currentUser.Id == post.Author.Id)
            {
                return View(postEdit);
            }
            TempData["message"] = "You cannot edit this item";
            return RedirectToAction(("Index"), new { id = blogId });



        }

        //POST: Edit
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit([Bind("PostId,Title,Content,BlogId, Created,IsCommentAllowed")] PostEditViewModel postEditViewModel)
        {
            if (ModelState.IsValid)
            {
                //Kall til metoden save i repository

                var post = new Post
                {
                    PostId = postEditViewModel.PostId,
                    Title = postEditViewModel.Title,
                    Content = postEditViewModel.Content,
                    Created = postEditViewModel.Created,
                    IsCommentAllowed = postEditViewModel.IsCommentAllowed,
                    Blog = _repository.GetBlogById(postEditViewModel.BlogId)
                };
                //find the owner (the person logged in)
                post.Author = await _manager.FindByNameAsync(User.Identity.Name);

                await _repository.UpdatePost(post, User);
                // _repository.Update(product);
                //tempdata
                TempData["message"] = string.Format("{0} has been updated", post.Title);

                return RedirectToAction(("Index"), new { id = postEditViewModel.BlogId });
            }

            return View(postEditViewModel);

        }


        // GET: Delete
        [Authorize]
        public async Task<IActionResult> Delete(int id, int blogId)
        {
            var post = _repository.GetPostById(id);

            if (post == null)
            {
                //tempdata
                TempData["message"] = "Item not found";
            }
            else
            {

                var currentUser = await _manager.FindByNameAsync(User.Identity.Name);
                if (currentUser.Id == post.Author.Id)
                {
                    await _repository.DeletePost(post, User);
                    //tempdata
                    TempData["message"] = string.Format("The post has been deleted");
                }
                else
                {
                    TempData["message"] = "You cannot delete this item";
                }
            }

            return RedirectToAction(("Index"), new { id = blogId });

        }
    }
}
