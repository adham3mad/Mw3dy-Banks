using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Mw3dy.Data;
using Mw3dy.Models;

namespace Mw3dy.Controllers
{
    public class BaseController : Controller
    {
        protected readonly AppDbContext _context;
        protected readonly IConfiguration _configuration;

        // Active user session properties
        public int CurrentUserId { get; private set; } = 1; // Default fallback to user 1
        public User? CurrentUser { get; private set; }
        public bool IsEmployee { get; private set; } = false;

        public BaseController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // 1. Check for token in query parameter
            string? token = context.HttpContext.Request.Query["token"];
            bool isFromQuery = !string.IsNullOrEmpty(token);

            // 2. If not in query, check in cookies
            if (string.IsNullOrEmpty(token))
            {
                context.HttpContext.Request.Cookies.TryGetValue("mw3dy-token", out token);
            }

            if (!string.IsNullOrEmpty(token))
            {
                var claimsPrincipal = ValidateToken(token);
                if (claimsPrincipal != null)
                {
                    // Token is valid! Parse claims and establish session
                    var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value ?? claimsPrincipal.FindFirst("email")?.Value;
                    var name = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value ?? claimsPrincipal.FindFirst("name")?.Value;
                    var role = claimsPrincipal.FindFirst(ClaimTypes.Role)?.Value ?? claimsPrincipal.FindFirst("role")?.Value;
                    var tokenUserIdStr = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? claimsPrincipal.FindFirst("sub")?.Value;

                    bool isEmployeeRole = string.Equals(role, "employee", StringComparison.OrdinalIgnoreCase) || 
                                          string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase);

                    // Sync/Get user from DB
                    var user = GetOrCreateUser(tokenUserIdStr, email, name, isEmployeeRole);
                    if (user != null)
                    {
                        CurrentUserId = user.Id;
                        CurrentUser = user;
                        IsEmployee = user.IsEmployee;

                        // Save to cookies if we just parsed it from query
                        if (isFromQuery)
                        {
                            var isHttps = context.HttpContext.Request.IsHttps;
                            var cookieOptions = new CookieOptions
                            {
                                Expires = DateTimeOffset.UtcNow.AddDays(7),
                                HttpOnly = true,
                                Secure = isHttps,
                                SameSite = SameSiteMode.Lax
                            };
                            context.HttpContext.Response.Cookies.Append("mw3dy-token", token, cookieOptions);
                            context.HttpContext.Response.Cookies.Append("mw3dy-user-id", user.Id.ToString(), new CookieOptions 
                            { 
                                Expires = DateTimeOffset.UtcNow.AddDays(7), 
                                HttpOnly = false, 
                                Secure = isHttps, 
                                SameSite = SameSiteMode.Lax 
                            });

                            // Redirect to the same URL without the token parameter to keep URL clean
                            var request = context.HttpContext.Request;
                            var queryParams = request.Query.Where(q => q.Key != "token").ToDictionary(q => q.Key, q => (string?)q.Value.ToString());
                            var redirectUrl = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(request.Path, queryParams);

                            context.Result = new RedirectResult(redirectUrl);
                            base.OnActionExecuting(context);
                            return;
                        }
                    }
                }
                else
                {
                    // Token is invalid/expired! Clear cookies
                    context.HttpContext.Response.Cookies.Delete("mw3dy-token");
                    context.HttpContext.Response.Cookies.Delete("mw3dy-user-id");
                }
            }
            else
            {
                // Fallback to mw3dy-user-id cookie for backward compatibility if no token is present
                if (context.HttpContext.Request.Cookies.TryGetValue("mw3dy-user-id", out var idStr) && int.TryParse(idStr, out var id))
                {
                    var user = _context.Users.FirstOrDefault(u => u.Id == id);
                    if (user != null)
                    {
                        CurrentUserId = user.Id;
                        CurrentUser = user;
                        IsEmployee = user.IsEmployee;
                    }
                }
            }

            // Expose user info to ViewBag for Layout rendering
            ViewBag.CurrentUserId = CurrentUserId;
            ViewBag.CurrentUser = CurrentUser;
            ViewBag.IsEmployee = IsEmployee;

            base.OnActionExecuting(context);
        }

        private ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var keyStr = _configuration["Jwt:Key"] ?? "SUPER_SECRET_KEY_MAWIDY_SYSTEM_2026_1234567890";
                var key = Encoding.UTF8.GetBytes(keyStr);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"] ?? "MawidyPlatform",
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"] ?? "MawidyPlatform",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                return tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private User? GetOrCreateUser(string? tokenUserIdStr, string? email, string? name, bool isEmployee)
        {
            if (string.IsNullOrEmpty(email))
            {
                email = !string.IsNullOrEmpty(tokenUserIdStr) ? $"{tokenUserIdStr}@external.com" : "external@mw3dy.com";
            }

            // Find user by Email
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            
            // If not found by email, and tokenUserIdStr is a valid integer, try to find by ID
            if (user == null && int.TryParse(tokenUserIdStr, out var parsedId))
            {
                user = _context.Users.FirstOrDefault(u => u.Id == parsedId);
            }

            if (user == null)
            {
                // Create user
                user = new User
                {
                    Name = name ?? email.Split('@')[0],
                    Email = email,
                    IsEmployee = isEmployee,
                    City = "New Cairo",
                    Address = "",
                    Phone = ""
                };

                // If parsedId is valid and doesn't conflict with seeded IDs (1, 2)
                if (int.TryParse(tokenUserIdStr, out var targetId) && targetId > 2)
                {
                    user.Id = targetId;
                }

                _context.Users.Add(user);
                _context.SaveChanges();
            }
            else
            {
                // Update properties if they changed
                bool updated = false;
                if (!string.IsNullOrEmpty(name) && user.Name != name)
                {
                    user.Name = name;
                    updated = true;
                }
                if (user.IsEmployee != isEmployee)
                {
                    user.IsEmployee = isEmployee;
                    updated = true;
                }

                if (updated)
                {
                    _context.SaveChanges();
                }
            }

            return user;
        }

        protected IActionResult? CheckAuthorization()
        {
            // If the user is not authenticated (i.e. no CurrentUser found), redirect to Login
            if (CurrentUser == null)
            {
                var loginUrl = _configuration["Jwt:LoginUrl"] ?? "/Home/Login";
                return Redirect(loginUrl);
            }
            return null;
        }
    }
}
