using Microsoft.AspNetCore.Identity;

namespace WebAPI.Models.Entities
{
    public class BlogOld
    {
        public int BlogId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public virtual IdentityUser Owner { get; set; }
        public bool IsPostAllowed { get; set; }
    }
}
