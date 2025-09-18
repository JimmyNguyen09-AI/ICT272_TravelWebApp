using ICT272_Project.Data;
using ICT272_Project.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
            if (!ModelState.IsValid) return View(user);

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
            await _context.SaveChangesAsync();

            // Nếu là Tourist -> tạo tourist profile
            if (string.Equals(user.Role, "Tourist", StringComparison.OrdinalIgnoreCase))
            {
                if (!_context.Tourists.Any(t => t.Email.ToLower() == normalizedEmail))
                {
                    _context.Tourists.Add(new Tourist
                    {
                        FullName = user.UserName,
                        Email = user.Email,
                        PasswordHash = hashedPassword,
                        UserID = user.UserID
                    });
                }
            }
            // Nếu là Agency -> tạo agency profile
            else if (string.Equals(user.Role, "Agency", StringComparison.OrdinalIgnoreCase))
            {
                if (!_context.TravelAgencies.Any(a => a.AgencyName.ToLower() == normalizedUserName))
                {
                    _context.TravelAgencies.Add(new TravelAgency
                    {
                        AgencyName = user.UserName,
                        ContactInfo = user.Email,
                        Description = "New agency created",
                        ServicesOffered = "Not specified yet",
                        ProfileImage = "Please update your picture",
                        UserID = user.UserID
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = _context.Users.FirstOrDefault(u =>
                u.UserName.ToLower() == model.UserName.ToLower());

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                ModelState.AddModelError("", "Invalid Username or Password");
                return View(model);
            }

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

            if (user.Role == "Agency")
                return RedirectToAction("Index", "TravelAgencies");
            else
                return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
