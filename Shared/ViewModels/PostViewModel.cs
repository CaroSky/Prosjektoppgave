﻿using Microsoft.AspNetCore.Identity;
using SharedModels.Entities;

namespace SharedModels.ViewModels
{

    public class PostIndexViewModel
    {
        public IEnumerable<Post> Posts { get; set; }
        public int BlogId { get; set; }
        public string BlogTitle { get; set; }
        public bool IsPostAllowed { get; set; }
        public int OwnerId { get; set; }
        public string ImageBase64 { get; set; }
        public Dictionary<int, int> LikesCount { get; set; } 
        public Dictionary<int, bool> UserLiked { get; set; }
        public Dictionary<int, List<string>> LikesByPostId { get; set; }
    }

    public class PostCreateViewModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public int BlogId { get; set; }
        public bool IsCommentAllowed { get; set; }
        public string ImageBase64 { get; set; }

    }

    public class PostEditViewModel
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public int BlogId { get; set; }
        public bool IsCommentAllowed { get; set; }
        public string ImageBase64 { get; set; }

    }
}
