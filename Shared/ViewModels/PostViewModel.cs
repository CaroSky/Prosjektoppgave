using Microsoft.AspNetCore.Identity;
using SharedModels.Entities;
using System.ComponentModel.DataAnnotations;

namespace SharedModels.ViewModels
{

    public class PostIndexViewModel
    {
        public IEnumerable<Post> Posts { get; set; }
        public int BlogId { get; set; }
        public string BlogTitle { get; set; }
        public bool IsPostAllowed { get; set; }
        public int OwnerId { get; set; }
        public bool IsInSearchMode { get; set; }
    }

    public class PostCreateViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }
        public int BlogId { get; set; }
        public bool IsCommentAllowed { get; set; }

    }

    public class PostEditViewModel
    {
        public int PostId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public int BlogId { get; set; }
        public bool IsCommentAllowed { get; set; }
        public string OwnerId { get; set; } 
        public virtual string OwnerUsername { get; set; }

    }
}
