using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mw3dy.Models;

namespace Mw3dy.Controllers
{
    public class HomeController : Controller
    {
        private readonly Data.AppDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(Data.AppDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            int userId = 1;
            if (Request.Cookies.TryGetValue("mw3dy-user-id", out var idStr) && int.TryParse(idStr, out var id))
            {
                userId = id;
            }
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null && user.IsEmployee)
            {
                return RedirectToAction("Index", "Employee");
            }
            return View();
        }

        public IActionResult SwitchUser(int id)
        {
            Response.Cookies.Append("mw3dy-user-id", id.ToString(), new CookieOptions 
            { 
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.Lax
            });
            
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user != null && user.IsEmployee)
            {
                return RedirectToAction("Index", "Employee");
            }
            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult SetLanguage(string culture)
        {
            if (!string.IsNullOrEmpty(culture))
            {
                Response.Cookies.Append(
                    "mw3dy-lang",
                    culture,
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddYears(1),
                        HttpOnly = false, // Allow client-side JS to read it as well if needed
                        Secure = true,
                        SameSite = SameSiteMode.Lax
                    }
                );
            }

            // Redirect back to the referrer page, or default to Index
            string? referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }
            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
