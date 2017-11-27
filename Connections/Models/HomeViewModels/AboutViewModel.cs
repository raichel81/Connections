using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Connections.Models;
using System.ComponentModel.DataAnnotations;


namespace Connections.Models.HomeViewModels
{
    public class AboutViewModel
    {
        public string Description { get; set; }
        public ICollection<ApplicationUser> Invites { get; set; }
        public ICollection<ApplicationUser> Friends { get; set; }
    }
}
