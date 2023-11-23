using Microsoft.AspNetCore.Identity;

namespace oblig2.Models.Entities
{
    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public Blog Blog { get; set;}
        public virtual IdentityUser Author { get; set; }
        public bool IsCommentAllowed { get; set; }


    }
}
