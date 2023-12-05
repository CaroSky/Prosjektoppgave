﻿using Microsoft.AspNetCore.Authorization;
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

        public async Task<PostIndexViewModel> GetPosts([FromRoute] int id)
        {
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



        // GET: Create
        //[Authorize]
        //public ActionResult Create(int id)
        //{
        //    var post = _repository.GetPostCreateViewModel(id);

        //    return View(post);
        //}

        //POST: Create
        [HttpPost]
        //[Authorize]
        public async Task<IActionResult> Post([FromBody] PostCreateViewModel postCreateViewModel)
        {
            //to be removed
           // var user = await _manager.FindByNameAsync(_username);
            //if (user != null)
           // {
              //  var claims = new List<Claim>
                //{
                  //  new Claim(ClaimTypes.Name, user.UserName),
                    // Add other claims as needed
                //};
               // var identity = new ClaimsIdentity(claims, "custom");
               // var principal = new ClaimsPrincipal(identity);

                // Set the principal to HttpContext.User
               // HttpContext.User = principal;
            //}
            //---------------------------------------------------------

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
                    IsCommentAllowed = true
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
        //[Authorize]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] PostEditViewModel postEditViewModel)
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
                Blog = await _repository.GetBlogById(postEditViewModel.BlogId)
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
            //to be removed
            var user = await _manager.FindByNameAsync(_username);
           // if (user != null)
            //{
              //  var claims = new List<Claim>
               // {
                 //   new Claim(ClaimTypes.Name, user.UserName),
                    // Add other claims as needed
                //};
                //var identity = new ClaimsIdentity(claims, "custom");
                //var principal = new ClaimsPrincipal(identity);

                // Set the principal to HttpContext.User
                //HttpContext.User = principal;
            //}
            //---------------------------------------------------------
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var post = await _repository.GetPostById(id);

            if (post == null)
            {
                return NotFound();
            }


           // var currentUser = await _manager.FindByNameAsync(User.Identity.Name);
            //if (currentUser.Id == post.Author.Id)
            //{
             await _repository.DeletePost(post, User);
            //await _repository.RemoveOrphanedTags();
            return Ok(post);
            //}
           //else
            //{
              //  return BadRequest(ModelState);
            //}
            
        }
    }
}
