using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using LibraryForProject.Models.Dashboard;

namespace LibraryForProject.Models.Authentication
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Post> Posts { get; set; } // The user's posts
        public List<ApplicationUser> Friends { get; set; } = new List<ApplicationUser>(); // Initialize the list

    }
}
