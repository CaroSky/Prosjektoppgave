using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace SharedModels.Entities
{
    public class Subscription
    {
        public int SubscriptionId { get; set; }
        public string UserId { get; set; } 
        public int BlogId { get; set; }

        // Navigation properties
        public virtual IdentityUser User { get; set; }
        public virtual Blog Blog { get; set; }
    }

}
