using LibraryForProject.Models.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryForProject.Models.Authentication
{
    public class UserProfileViewModel
    {
        public ApplicationUser User { get; set; }
        public List<Post> Posts { get; set; }  // List of posts by the user or their friends
        public List<Post> FriendsPosts { get; set; } // List of posts by friends
    }
}
