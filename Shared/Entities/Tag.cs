using SharedModels.Entities;

namespace SharedModels.Entities
{
    public class Tag
    {
        public int TagId { get; set; }
        public string Name { get; set; }
        //public ICollection<Post> Posts { get; set; }
    }
}