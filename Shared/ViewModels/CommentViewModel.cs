using Microsoft.AspNetCore.Identity;

using SharedModels.Entities;

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
        public string Content { get; set; }
        public int PostId { get; set; }

    }

    public class CommentEditViewModel
    {
        public int CommentId { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public int PostId { get; set; }

    }
}