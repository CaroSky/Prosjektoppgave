using Microsoft.AspNetCore.Identity;
using oblig2.Models.Entities;

namespace oblig2.Models.ViewModels
{

    public class PostIndexViewModel
    {
        public IEnumerable<Post> Posts { get; set; }
        public int BlogId { get; set; }
        public string BlogTitle { get; set; }
        public bool IsPostAllowed { get; set; }
    }

    public class PostCreateViewModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public int BlogId { get; set; }
        public bool IsCommentAllowed { get; set; }

    }

    public class PostEditViewModel
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public int BlogId { get; set; }
        public bool IsCommentAllowed { get; set; }

    }
}
