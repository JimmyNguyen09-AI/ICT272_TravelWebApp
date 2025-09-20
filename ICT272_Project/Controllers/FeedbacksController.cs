using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ICT272_Project.Data;
using ICT272_Project.Models;

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
            var feedbacksQuery = _context.Feedbacks
                .Include(f => f.Tourist)
                .AsQueryable();

            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userIdValue) && User.IsInRole("Tourist") && int.TryParse(userIdValue, out var userId))
            {
                feedbacksQuery = feedbacksQuery.Where(f => f.Tourist != null && f.Tourist.UserID == userId);
            }

            var feedbacks = await feedbacksQuery
                .OrderByDescending(f => f.Date)
                .ToListAsync();

            var bookingLookup = new Dictionary<int, string>();
            var bookingIds = feedbacks.Select(f => f.BookingID).Distinct().ToList();

            if (bookingIds.Any())
            {
                bookingLookup = await _context.Booking
                    .Where(b => bookingIds.Contains(b.BookingID))
                    .Include(b => b.TourPackage)
                    .ToDictionaryAsync(
                        b => b.BookingID,
                        b => string.IsNullOrWhiteSpace(b.TourPackage?.Title)
                            ? $"Booking #{b.BookingID}"
                            : b.TourPackage.Title);
            }

            ViewBag.BookingLookup = bookingLookup;

            return View(feedbacks);
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
        public async Task<IActionResult> Create(int? bookingId)
        {
            var tourist = await GetCurrentTouristAsync();
            if (tourist == null)
            {
                return RedirectToAction("Login", "Account");
            }

            await PopulateApprovedBookingsAsync(tourist.TouristID, bookingId);

            var feedback = new Feedback
            {
                BookingID = bookingId ?? 0,
                TouristID = tourist.TouristID
            };

            return View(feedback);
        }

        // POST: Feedbacks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingID,Rating,Comment")] Feedback feedback)
        {
            var tourist = await GetCurrentTouristAsync();
            if (tourist == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ModelState.Remove(nameof(Feedback.TouristID));
            ModelState.Remove(nameof(Feedback.Date));

            var approvedBooking = await _context.Booking
                .FirstOrDefaultAsync(b =>
                    b.BookingID == feedback.BookingID &&
                    b.TouristID == tourist.TouristID);
            var approvedStatus = approvedBooking?.Status?.Trim();
            var normalizedApprovedStatus = approvedStatus?.ToUpperInvariant();

            if (approvedBooking == null ||
               !string.Equals(normalizedApprovedStatus, "APPROVED", StringComparison.Ordinal))
            {
                ModelState.AddModelError("BookingID", "Please select an approved booking that belongs to you.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateApprovedBookingsAsync(tourist.TouristID, feedback.BookingID);
                return View(feedback);
            }

            feedback.TouristID = tourist.TouristID;
            feedback.Date = DateTime.UtcNow;

            _context.Add(feedback);
            await _context.SaveChangesAsync();

            TempData["FeedbackSuccess"] = "Thank you! Your feedback has been submitted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<Tourist?> GetCurrentTouristAsync()
        {
            if (!User.Identity?.IsAuthenticated ?? true) return null;

            if (!User.IsInRole("Tourist")) return null;

            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdValue) || !int.TryParse(userIdValue, out var userId))
            {
                return null;
            }

            return await _context.Tourists.FirstOrDefaultAsync(t => t.UserID == userId);
        }

        private async Task PopulateApprovedBookingsAsync(int touristId, int? selectedBookingId)
        {
            var approvedBookings = await _context.Booking
                .Where(b =>
                    b.TouristID == touristId &&
                    b.Status != null &&
                    b.Status.Trim().ToUpper() == "APPROVED")
                .Include(b => b.TourPackage)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            var bookingItems = approvedBookings
                .Select(b => new SelectListItem
                {
                    Value = b.BookingID.ToString(),
                    Text = string.IsNullOrWhiteSpace(b.TourPackage?.Title)
                        ? $"Booking #{b.BookingID}"
                        : $"#{b.BookingID} - {b.TourPackage.Title}",
                    Selected = selectedBookingId.HasValue && selectedBookingId.Value == b.BookingID
                })
                .ToList();

            ViewBag.Bookings = bookingItems;
            ViewBag.HasApprovedBookings = bookingItems.Any();
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

        private bool FeedbackExists(int id)
        {
            return _context.Feedbacks.Any(e => e.FeedbackID == id);
        }
    }
}
