using System;
using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mw3dy.Models;

namespace Mw3dy.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(Data.AppDbContext context, ILogger<HomeController> logger, IConfiguration configuration)
            : base(context, configuration)
        {
            _logger = logger;
        }   

        public IActionResult Index()
        {
            if (CurrentUser != null)
            {
                if (IsEmployee)
                    return RedirectToAction("Index", "Employee");
                else
                    return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Logout()
        {
            Response.Cookies.Delete("mw3dy-token");
            Response.Cookies.Delete("mw3dy-user-id");

            var loginUrl = _configuration["Jwt:LoginUrl"] ?? "/Home/Login";
            // If the loginUrl is local (starts with /), redirect there. Otherwise, redirect to the external URL.
            if (loginUrl.StartsWith("/"))
            {
                return RedirectToAction("Login", "Home");
            }
            return Redirect(loginUrl);
        }

        public IActionResult GenerateTestToken(int userId = 1, string role = "customer", string name = "Adham Emad", string email = "adham@mw3dy.com")
        {
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var keyStr = _configuration["Jwt:Key"] ?? "SUPER_SECRET_KEY_MAWIDY_SYSTEM_2026_1234567890";
            var key = Encoding.UTF8.GetBytes(keyStr);
            var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString()),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, name),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, email),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = _configuration["Jwt:Issuer"] ?? "MawidyPlatform",
                Audience = _configuration["Jwt:Audience"] ?? "MawidyPlatform",
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key), Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            var scheme = Request.Scheme;
            var host = Request.Host.ToUriComponent();
            var redirectUrl = $"{scheme}://{host}/?token={tokenString}";

            return Content($"Token:\n{tokenString}\n\nUse this link to test JWT SSO Login:\n{redirectUrl}");
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
                        HttpOnly = false,
                        Secure = true,
                        SameSite = SameSiteMode.Lax
                    }
                );
            }

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
