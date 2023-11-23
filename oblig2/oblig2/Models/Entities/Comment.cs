using Microsoft.AspNetCore.Identity;

namespace oblig2.Models.Entities
{
    public class Comment
    {
        public int CommentId { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public Post Post { get; set; }
        public virtual IdentityUser Author { get; set; }


    }
}
