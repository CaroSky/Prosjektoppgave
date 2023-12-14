using Microsoft.AspNetCore.Identity;

using SharedModels.Entities;
using System.ComponentModel.DataAnnotations;

namespace SharedModels.ViewModels
{

    public class CommentIndexViewModel
    {
        public IEnumerable<Comment> Comments { get; set; }
        public int PostId { get; set; }
        public int BlogId { get; set; }
        public string PostTitle { get; set; }
        public bool IsCommentAllowed { get; set; }
    }
    public class CommentCreateViewModel
    {
        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }
        public int PostId { get; set; }

    }

    public class CommentEditViewModel
    {
        public int CommentId { get; set; }

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public int PostId { get; set; }
        public string OwnerId { get; set; } 
        public virtual string OwnerUsername { get; set; }

    }
}