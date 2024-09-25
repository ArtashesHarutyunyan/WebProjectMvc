using LibraryForProject.Models.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using LibraryForProject.Models;
using LibraryForProject.Models.Dashboard;



namespace WebProjectMvc.Controllers
{

    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly AppDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger, AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Username, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Account");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Если что-то пошло не так, повторно отобразить форму
            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return RedirectToAction("AccountHomePage", "Account");
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return View("Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            // Если модель не валидна, повторно отобразить форму
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
        public async Task<IActionResult> AccountHomePage()
        {
            // Get the current logged-in user
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            List<Post> userPosts = new List<Post>();
            List<Post> friendsPosts = new List<Post>();

            try
            {
                // Retrieve user's posts
                userPosts = await _context.Posts
                    .Where(p => p.UserId == user.Id)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                // Retrieve friends' posts
                friendsPosts = await _context.Posts
                    .Where(p => user.Friends.Select(f => f.Id).Contains(p.UserId))
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving posts for user {UserId}", user.Id);
                // Optionally, you can add a message to the model to display in the view
                ViewBag.ErrorMessage = "There was an error retrieving your posts.";
            }

            var viewModel = new UserProfileViewModel
            {
                User = user,
                Posts = userPosts,
                FriendsPosts = friendsPosts // Include friends' posts in the view model
            };

            return View(viewModel);
        }
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return View("Search", null);  // If no query, return empty results
            }

            // Search for users by username or email
            var users = _userManager.Users
                .Where(u => u.UserName.Contains(query) || u.Email.Contains(query))
                .ToList();

            return View("Search", users);
        }

        public async Task<IActionResult> FriendsPosts()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var friendsPosts = await _context.Posts
                .Where(p => currentUser.Friends.Contains(p.User))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(friendsPosts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError(string.Empty, "Post content cannot be empty.");
                return RedirectToAction("AccountHomePage"); // Redirect to the homepage
            }

            var user = await _userManager.GetUserAsync(User);
            var post = new Post // Assuming you have a Post model
            {
                UserId = user.Id,
                Content = content,
                CreatedAt = DateTime.UtcNow // Set the creation time
            };

            try
            {
                _context.Posts.Add(post);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post for user {UserId}", user.Id);
                ModelState.AddModelError(string.Empty, "There was an error creating your post.");
                return RedirectToAction("AccountHomePage"); // Redirect back to the homepage
            }

            return RedirectToAction("AccountHomePage"); // Redirect back to the homepage
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFriend(string friendUsername)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var friend = await _userManager.FindByNameAsync(friendUsername);

            if (friend != null && currentUser != null)
            {
                // Add logic to add the friend to the user's friends list
                currentUser.Friends.Add(friend);
                await _userManager.UpdateAsync(currentUser);
                _logger.LogInformation($"User {currentUser.UserName} added {friend.UserName} as a friend.");
            }

            return RedirectToAction("AccountHomePage");
        }
        public async Task<IActionResult> ViewProfile(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            return View("ViewProfile", user);
        }

    }
}
