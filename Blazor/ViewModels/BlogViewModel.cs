using System.ComponentModel.DataAnnotations;

namespace Blazor.ViewModels
{
    public class BlogViewModel
    {
        public int BlogId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public bool IsPostAllowed { get; set; }

    }
}
