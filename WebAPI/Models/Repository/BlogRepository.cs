﻿using Microsoft.AspNetCore.Identity;
using WebAPI.Data;
using System.Security.Principal;
using WebAPI.Models.Entities;
using WebAPI.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using SharedModels.Entities;
using SharedModels.ViewModels;

namespace WebAPI.Models.Repositories
{
    public interface IBlogRepository
    {
        Task<IEnumerable<Blog>> GetAllBlogs();
        Task SaveBlog(Blog blog, IPrincipal principal);
        //BlogCreateViewModel GetBlogCreateViewModel();
        //BlogEditViewModel GetBlogEditViewModel();
        Task<BlogEditViewModel> GetBlogEditViewModelById(int Id);
        Task<Blog> GetBlogById(int Id);
        Task UpdateBlog(Blog blog);
        Task DeleteBlog(Blog blog, IPrincipal principal);
        Task<IEnumerable<Post>> GetAllPostByBlogId(int id);
        Task SavePost(Post post, IPrincipal principal);
        //PostCreateViewModel GetPostCreateViewModel(int blogId);
        //PostEditViewModel GetPostEditViewModel(int blogId);
        Task<PostEditViewModel> GetPostEditViewModelById(int blogId);
        Task<Post> GetPostById(int Id);
        Task UpdatePost(Post post, IPrincipal principal);
        Task DeletePost(Post post, IPrincipal principal);

        Task<IEnumerable<Comment>> GetAllCommentsByPostId(int postId);
        Task SaveComment(Comment comment, IPrincipal principal);
        //CommentCreateViewModel GetCommentCreateViewModel(int postId);
        //CommentEditViewModel GetCommentEditViewModel(int postId);
        Task<CommentEditViewModel> GetCommentEditViewModelById(int commentId);
        Task<Comment> GetCommentById(int Id);
        Task UpdateComment(Comment comment, IPrincipal principal);
        Task DeleteComment(Comment comment, IPrincipal principal);
        Task<IEnumerable<Tag>> GetTags();
        Task<Tag> GetTagByName(string name);
        Task<IEnumerable<Post>> SearchPostByTag(string name);

    }


    public class BlogRepository : IBlogRepository
    {
        private ApplicationDbContext _db;
        private UserManager<IdentityUser> _manager;
        private readonly ILogger<BlogRepository> _logger;

        public BlogRepository(UserManager<IdentityUser> userManager, ApplicationDbContext db
            , ILogger<BlogRepository> logger)
        {
            this._db = db;
            this._manager = userManager;
            _logger = logger;
        }

        //blog------------------------------------------------------------------------------------------------

        public async Task<IEnumerable<Blog>> GetAllBlogs()
        {
            //var blogs = _db.Blog.Include(b => b.Owner).ToList();
            //return blogs;
            try
            {
                _logger.LogInformation("Getting all blogs");
                var blogs = await _db.Blog.ToListAsync();
                return blogs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all blogs");
                throw;
            }
        }

        //public BlogCreateViewModel GetBlogCreateViewModel()
        //{
        //    var blog = new BlogCreateViewModel();
        //    blog.IsPostAllowed = true;
        //    return blog;
        //}

        public async Task SaveBlog(Blog blog, IPrincipal principal)
        {
            try
            {
                _db.Blog.Add(blog);
                _db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        //public BlogEditViewModel GetBlogEditViewModel()
        //{
        //    var blog = new BlogEditViewModel();
        //    return blog;
        //}

        public async Task<BlogEditViewModel> GetBlogEditViewModelById(int Id)
        {
            var blogs = _db.Blog.ToList();
            var blog = blogs.Where(b => b.BlogId == Id).First();
            ;

            var editBlog = new BlogEditViewModel
            {
                BlogId = blog.BlogId,
                Title = blog.Title,
                Content = blog.Content,
                Created = blog.Created,
                IsPostAllowed = blog.IsPostAllowed,
            };

            return editBlog;
        }

        public async Task<Blog> GetBlogById(int blogId)
        {
            //var blogs = _db.Blog.Include(item => item.Owner).ToList();
            //var blog = blogs.Where(item => item.BlogId == blogId).First(); ;

            // return blog;

            // Anta at _db er din DbContext-instans
            var blog = await _db.Blog
                .SingleOrDefaultAsync(item =>
                    item.BlogId ==
                    blogId); // Bruker SingleOrDefaultAsync for å håndtere tilfeller hvor det ikke finnes en matchende blog

            // Hvis du trenger brukerdetaljer (som brukernavn), må du hente det separat siden Blog ikke lenger inneholder Owner
            // Dette kan gjøres ved å slå opp brukeren basert på UserId
            if (blog != null)
            {
                var user = await _db.Users
                    .SingleOrDefaultAsync(u => u.Id == blog.OwnerId); // Anta at 'Users' er tabellen for brukere
                // Du kan nå legge til brukerdetaljer til blog-objektet hvis det trengs, f.eks.:
                // blog.UserName = user?.UserName;
            }

            return blog;
        }

        public async Task UpdateBlog(Blog blog)
        {
            _db.Blog.Update(blog);
            _db.SaveChanges();
        }

        // public void Delete(Product product)
        public async Task DeleteBlog(Blog blog, IPrincipal principal)
        {

            var posts = await GetAllPostByBlogId(blog.BlogId);

            foreach (var post in posts)
            {
                await DeletePost(post, principal);
            }

            _db.Blog.Remove(blog);
            _db.SaveChanges();
        }



        //post---------------------------------------------------------------------------------------

        public async Task<IEnumerable<Post>> GetAllPostByBlogId(int id)
        {
            var allPosts = _db.Post.Include(item => item.Blog).ToList();
            // var allPosts = _db.Post.Include(item => item.Blog).Include(item => item.Author).ToList();
            var posts = allPosts.Where(item => item.Blog.BlogId == id);
            return posts;
        }

        //public PostCreateViewModel GetPostCreateViewModel(int blogId)
        //{
        //    var post = new PostCreateViewModel();
        //    post.BlogId = blogId;
        //    post.IsCommentAllowed = true;
        //    return post;
        //}

        public async Task SavePost(Post post, IPrincipal principal)
        {
            try
            {
                _db.Post.Add(post);
                _db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        //public PostEditViewModel GetPostEditViewModel(int blogId)
        //{
        //    var post = new PostEditViewModel();
        //    post.BlogId = blogId;
        //    return post;
        //}

        public async Task<PostEditViewModel> GetPostEditViewModelById(int PostId)
        {
            var posts = _db.Post.Include(item => item.Blog).ToList();
            var post = posts.Where(item => item.PostId == PostId).First();
            ;

            var editPost = new PostEditViewModel
            {
                PostId = post.PostId,
                Title = post.Title,
                Content = post.Content,
                Created = post.Created,
                BlogId = post.Blog.BlogId,
                IsCommentAllowed = post.IsCommentAllowed,
            };

            return editPost;
        }

        public async Task<Post> GetPostById(int PostId)
        {
            //var posts = _db.Post.Include(item => item.Blog).Include(item => item.Author).ToList();
            var posts = _db.Post.Include(item => item.Blog).ToList();

            var post = posts.Where(item => item.PostId == PostId).First();
            ;

            return post;
        }

        public async Task UpdatePost(Post post, IPrincipal principal)
        {
            _db.Post.Update(post);
            _db.SaveChanges();
        }


        public async Task DeletePost(Post post, IPrincipal principal)
        {
            var comments = await GetAllCommentsByPostId(post.PostId);

            foreach (var comment in comments)
            {
                await DeleteComment(comment, principal);
            }

            _db.Post.Remove(post);
            _db.SaveChanges();
        }

        //comment-----------------------------------------------------------

        public async Task<IEnumerable<Comment>> GetAllCommentsByPostId(int postId)
        {
            var allComments = _db.Comment.Include(item => item.Post).Include(item => item.Author).ToList();
            var comments = allComments.Where(item => item.Post.PostId == postId);
            return comments;
        }

        //public CommentCreateViewModel GetCommentCreateViewModel(int postId)
        //{
        //    var comment = new CommentCreateViewModel();
        //    comment.PostId = postId;
        //    return comment;
        //}

        public async Task SaveComment(Comment comment, IPrincipal principal)
        {
            try
            {
                _db.Comment.Add(comment);
                _db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        //public CommentEditViewModel GetCommentEditViewModel(int postId)
        //{
        //    var comment = new CommentEditViewModel();
        //    comment.PostId = postId;
        //    return comment;
        //}

        public async Task<CommentEditViewModel> GetCommentEditViewModelById(int commentId)
        {
            var comments = _db.Comment.Include(item => item.Post).ToList();
            var comment = comments.Where(item => item.CommentId == commentId).First();
            ;

            var editComment = new CommentEditViewModel
            {
                CommentId = comment.CommentId,
                Content = comment.Content,
                Created = comment.Created,
                PostId = comment.Post.PostId
            };

            return editComment;
        }

        public async Task<Comment> GetCommentById(int CommentId)
        {
            var comments = _db.Comment.Include(item => item.Post).Include(item => item.Author).ToList();
            var comment = comments.Where(item => item.CommentId == CommentId).First();
            ;

            return comment;
        }

        public async Task UpdateComment(Comment comment, IPrincipal principal)
        {
            _db.Comment.Update(comment);
            _db.SaveChanges();
        }


        public async Task DeleteComment(Comment comment, IPrincipal principal)
        {
            _db.Comment.Remove(comment);
            _db.SaveChanges();
        }


        // Search tag---------------------------------------------------------------------------------------------

        public async Task<IEnumerable<Tag>> GetTags()
        {
            try
            {
                _logger.LogInformation("Getting all tags");
                var tags = await _db.Tag.ToListAsync();

                return tags;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all blogs");
                throw;
            }

        }


        public async Task<Tag> GetTagByName(string name)
        {
            var tags = _db.Tag.ToList();
            var tag = tags.Where(item => item.Name == name).FirstOrDefault();

            return tag;
        }

        public async Task<IEnumerable<Post>> SearchPostByTag(string name)
        {
            var tag = await GetTagByName(name);
            if (tag == null)
            {
                return new List<Post>();
            }
            var postIdList = _db.PostTag.Where(item => item.TagsTagId == tag.TagId).ToList();
            List<Post> postsList = new List<Post>();
            foreach (var postTag in postIdList)
            {
                postsList.Add(await GetPostById(postTag.PostsPostId));
            }

            return postsList;
        }

    }

}