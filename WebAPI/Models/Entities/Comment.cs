using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;

namespace WebAPI.Models.Entities
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
