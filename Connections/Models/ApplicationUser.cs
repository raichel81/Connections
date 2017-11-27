using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Connections.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser<int>
    {
        public static object Identity { get; internal set; }
        public string Name { get; set; }
        public string Description { get; set; }
        //Navigational Property 
        public virtual ICollection<ApplicationUser> Friends { get; set; }

    }
    public class ApplicationRole : IdentityRole<int>
    {
    }
}
