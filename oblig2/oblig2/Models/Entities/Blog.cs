using Microsoft.AspNetCore.Identity;

namespace oblig2.Models.Entities
{
    public class Blog
    {
        public int BlogId { get; set; }
        public string Title { get; set;}
        public string Content { get; set;}
        public DateTime Created { get; set;}
        public virtual IdentityUser Owner { get; set;}
        public bool IsPostAllowed { get; set;}

    }

}

