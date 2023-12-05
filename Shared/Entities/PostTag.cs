using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Entities
{
    public class PostTag
    {
        public int PostsPostId { get; set; }
        public int TagsTagId { get; set; }

    }
}
