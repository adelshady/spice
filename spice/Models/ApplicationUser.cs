using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace spice.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; }
     
        public string StreetAddress { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postalcode { get; set; }
    }
}
