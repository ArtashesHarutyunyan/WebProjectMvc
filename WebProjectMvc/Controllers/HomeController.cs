using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.SqlServer;
using System.Diagnostics;
using LibraryForProject.Models.Authentication;



namespace WebProjectMvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(ApplicationUser user)
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }



    }
}
