using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using WebAPI.Models.Entities;
using WebAPI.Models.Repositories;
using WebAPI.Models.ViewModels;
using System.Reflection.Metadata;

namespace WebAPI.Controllers
{
    public class CommentController : Controller
    {
        private IBlogRepository _repository;

        private UserManager<IdentityUser> _manager;



        public CommentController(UserManager<IdentityUser> manager, IBlogRepository repository)
        {
            this._repository = repository;
            this._manager = manager;
        }
        public IActionResult Index([FromRoute] int id)
        {

            var comments = _repository.GetAllCommentsByPostId(id);
            var post = _repository.GetPostById(id);
            var commentIndexViewModel = new CommentIndexViewModel
            {
                Comments = comments,
                PostId = id,
                BlogId = post.Blog.BlogId,
                PostTitle = post.Title,
                IsCommentAllowed = post.IsCommentAllowed,
            };

            return View(commentIndexViewModel);
        }

        // GET: Create
        [Authorize]
        public ActionResult Create(int id)
        {
            //this.blog = _repository.GetBlogById(id);
            var comment = _repository.GetCommentCreateViewModel(id);

            return View(comment);
        }

        //POST: Create
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Content,PostId")] CommentCreateViewModel commentCreateViewModel)
        {

            if (ModelState.IsValid)
            {
                //Kall til metoden save i repository
                var comment = new Comment()
                {
                    Content = commentCreateViewModel.Content,
                    Created = DateTime.Now,
                    Author = await _manager.FindByNameAsync(User.Identity.Name),
                    Post = _repository.GetPostById(commentCreateViewModel.PostId)
                };


                await _repository.SaveComment(comment, User);
                //tempdata
                TempData["message"] = string.Format("The comment has been created");

                return RedirectToAction(("Index"), new { id = commentCreateViewModel.PostId });
            }

            return View(commentCreateViewModel);

        }


        // GET: Edit
        [Authorize]
        public async Task<IActionResult> Edit(int id, int postId)
        {


            //sjekk om id finnes i product list
            var comment = _repository.GetCommentById(id);
            //var id = comment.Post.PostId;
            if (comment == null)
            {
                //tempdata
                TempData["message"] = "Item not found";
                return RedirectToAction(("Index"), new { id = postId });
            }

            var commentEdit = _repository.GetCommentEditViewModelById(id);


            var currentUser = await _manager.FindByNameAsync(User.Identity.Name);
            if (currentUser.UserName == comment.Author.UserName)
            {
                return View(commentEdit);
            }
            TempData["message"] = "You cannot edit this item";
            return RedirectToAction(("Index"), new { id = postId });


        }

        //POST: Edit
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit([Bind("CommentId,Content,PostId,Created")] CommentEditViewModel commentEditViewModel)
        {
            if (ModelState.IsValid)
            {
                //Kall til metoden save i repository

                var comment = new Comment()
                {
                    CommentId = commentEditViewModel.CommentId,
                    Content = commentEditViewModel.Content,
                    Created = commentEditViewModel.Created,
                    Post = _repository.GetPostById(commentEditViewModel.PostId)
                };
                //find the owner (the person logged in)
                comment.Author = await _manager.FindByNameAsync(User.Identity.Name);

                await _repository.UpdateComment(comment, User);
                // _repository.Update(product);
                //tempdata
                TempData["message"] = string.Format("The comment has been updated");

                return RedirectToAction(("Index"), new { id = commentEditViewModel.PostId });
            }

            return View(commentEditViewModel);

        }


        // GET: Delete
        [Authorize]
        public async Task<IActionResult> Delete(int id, int postId)
        {
            var comment = _repository.GetCommentById(id);
            //var id = comment.Post.PostId;

            if (comment == null)
            {
                //tempdata
                TempData["message"] = "Item not found";
            }
            else
            {
                var currentUser = await _manager.FindByNameAsync(User.Identity.Name);
                if (currentUser.UserName == comment.Author.UserName)
                {
                    await _repository.DeleteComment(comment, User);
                    //tempdata
                    TempData["message"] = string.Format("The comment has been deleted");
                }
                else
                {
                    TempData["message"] = "You cannot delete this item";
                }

            }

            return RedirectToAction(("Index"), new { id = postId });

        }
    }
}