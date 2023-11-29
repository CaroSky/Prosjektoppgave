using Microsoft.AspNetCore.Identity;
using WebAPI.Models.Entities;
using SharedModels.Entities;

namespace WebAPI.Models.ViewModels
{


    public class BlogCreateViewModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsPostAllowed { get; set; }


    }

    public class BlogEditViewModel
    {
        public int BlogId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public bool IsPostAllowed { get; set; }

    }
}