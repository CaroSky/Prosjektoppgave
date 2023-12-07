using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using SharedModels.Entities;

namespace SharedModels.Entities
{
    public class Comment
    {
        public int CommentId { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public Post Post { get; set; }
        public string OwnerId { get; set; } // Tove: Fremmednøkkel for brukeren
        public virtual string OwnerUsername { get; set; }
    }
}
