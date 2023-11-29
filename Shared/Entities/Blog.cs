

namespace SharedModels.Entities
{
    public class Blog
    {
        public int BlogId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public string UserId { get; set; } // Tove: Fremmednøkkel for brukeren
        public bool IsPostAllowed { get; set; }
    }
}
