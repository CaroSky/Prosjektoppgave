using Microsoft.AspNetCore.Identity;
using SharedModels.Entities;

namespace WebAPI.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {

        public virtual ICollection<Blog> Blogs { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    
}
