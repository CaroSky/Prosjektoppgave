using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SharedModels.Entities;


namespace SharedModels.Entities
{

    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public Blog Blog { get; set; }
        public string OwnerId { get; set; } // Tove: Fremmednøkkel for brukeren
        public virtual string OwnerUsername { get; set; }
        public bool IsCommentAllowed { get; set; }
        public int CountLike { get; set; }
        public string ImageBase64 { get; set; }

    }
    
}
