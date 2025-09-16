using ICT272_Project.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using ICT272_Project.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace ICT272_Project.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        public AccountController(AppDbContext context) => _context = context;

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Register(User user)
        {
            if(!ModelState.IsValid) return View(user);
            user.UserName = user.UserName?.Trim() ?? string.Empty;
            user.Email = user.Email?.Trim() ?? string.Empty;

            var normalizedEmail = user.Email.ToLower();
            var normalizedUserName = user.UserName.ToLower();

            if (_context.Users.Any(u => u.Email.ToLower() == normalizedEmail))
            {
                ModelState.AddModelError("", "Email already exists");
                return View(user);
            }
            if (_context.Users.Any(u => u.UserName.ToLower() == normalizedUserName))
            {
                ModelState.AddModelError("", "Username already exists");
                return View(user);
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.Password = hashedPassword;
            _context.Users.Add(user);
            if (string.Equals(user.Role, "Tourist", StringComparison.OrdinalIgnoreCase))
            {
                var existingTourist = _context.Tourists.FirstOrDefault(t => t.Email.ToLower() == normalizedEmail);
                if (existingTourist == null)
                {
                    _context.Tourists.Add(new Tourist
                    {
                        FullName = user.UserName,
                        Email = user.Email,
                        PasswordHash = hashedPassword
                    });
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login() => View(new LoginViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            model.UserName = model.UserName?.Trim() ?? string.Empty;

            var user = _context.Users.FirstOrDefault(u =>
                u.UserName.ToLower() == model.UserName.ToLower());

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                ModelState.AddModelError("", "Invalid Username or Password");
                return View(model);
            }
            await EnsureTouristProfileAsync(user);
            var claims = new List<Claim>
     {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }



        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
        private async Task EnsureTouristProfileAsync(User user)
        {
            if (!string.Equals(user.Role, "Tourist", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return;
            }

            var normalizedEmail = user.Email.Trim().ToLower();
            var existingTourist = _context.Tourists.FirstOrDefault(t => t.Email.ToLower() == normalizedEmail);
            if (existingTourist != null)
            {
                return;
            }

            _context.Tourists.Add(new Tourist
            {
                FullName = user.UserName,
                Email = user.Email,
                PasswordHash = user.Password
            });

            await _context.SaveChangesAsync();
        }
    }
}
