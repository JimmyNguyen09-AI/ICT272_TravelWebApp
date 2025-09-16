using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ICT272_Project.Data;
using ICT272_Project.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace ICT272_Project.Controllers
{
    public class FeedbacksController : Controller
    {
        private readonly AppDbContext _context;

        public FeedbacksController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Feedbacks
        public async Task<IActionResult> Index()
        {
            return View(await _context.Feedbacks
               .Include(f => f.Tourist)
               .OrderByDescending(f => f.Date)
               .ToListAsync());
        }

        // GET: Feedbacks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedback = await _context.Feedbacks
                .FirstOrDefaultAsync(m => m.FeedbackID == id);
            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }

        // GET: Feedbacks/Create
        [Authorize(Roles = "Tourist")]
        public async Task<IActionResult> Create()
        {
            var tourist = await GetOrCreateCurrentTouristAsync();
            if (tourist == null)
            {
                TempData["FeedbackError"] = "Unable to locate a tourist profile for your account.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.TouristName = tourist.FullName;
            return View(new Feedback
            {
                TouristID = tourist.TouristID
            });
        }

        // POST: Feedbacks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Tourist")]
        public async Task<IActionResult> Create([Bind("FeedbackID,BookingID,Rating,Comment")] Feedback feedback)
        {
            var tourist = await GetOrCreateCurrentTouristAsync();
            if (tourist == null)
            {
                TempData["FeedbackError"] = "Unable to locate a tourist profile for your account.";
                return RedirectToAction(nameof(Index));
            }

            feedback.TouristID = tourist.TouristID;
            feedback.Date = DateTime.UtcNow;

            ViewBag.TouristName = tourist.FullName;

            ModelState.Remove(nameof(Feedback.TouristID));
            ModelState.Remove(nameof(Feedback.Tourist));
            ModelState.Remove(nameof(Feedback.Date));
            if (ModelState.IsValid)
            {
                _context.Add(feedback);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(feedback);
        }

        // GET: Feedbacks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return NotFound();
            }
            return View(feedback);
        }

        // POST: Feedbacks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FeedbackID,BookingID,TouristID,Rating,Comment,Date")] Feedback feedback)
        {
            if (id != feedback.FeedbackID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(feedback);
                    await _context.SaveChangesAsync();
                    TempData["FeedbackMessage"] = "Thank you for sharing your feedback!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FeedbackExists(feedback.FeedbackID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(feedback);
        }

        // GET: Feedbacks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedback = await _context.Feedbacks
                .FirstOrDefaultAsync(m => m.FeedbackID == id);
            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }

        // POST: Feedbacks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback != null)
            {
                _context.Feedbacks.Remove(feedback);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FeedbackExists(int id)
        {
            return _context.Feedbacks.Any(e => e.FeedbackID == id);
        }
        private async Task<Tourist?> GetOrCreateCurrentTouristAsync()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            var normalizedEmail = email.Trim().ToLower();
            var tourist = await _context.Tourists.FirstOrDefaultAsync(t => t.Email.ToLower() == normalizedEmail);
            if (tourist != null)
            {
                return tourist;
            }

            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out var userId))
            {
                return null;
            }

            var appUser = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
            if (appUser == null || !string.Equals(appUser.Role, "Tourist", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            tourist = new Tourist
            {
                FullName = appUser.UserName,
                Email = appUser.Email,
                PasswordHash = appUser.Password
            };

            _context.Tourists.Add(tourist);
            await _context.SaveChangesAsync();

            return tourist;
        }
    }
}
