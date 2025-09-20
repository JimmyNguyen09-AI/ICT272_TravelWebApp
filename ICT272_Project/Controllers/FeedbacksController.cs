using System.Security.Claims;
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

            if (!string.IsNullOrEmpty(userIdValue) && User.IsInRole("Tourist") &&
                int.TryParse(userIdValue, out var userId))
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

        // GET: Feedbacks/Create
        public async Task<IActionResult> Create(int? bookingId)
        {
            var tourist = await GetCurrentTouristAsync();
            if (tourist == null) return RedirectToAction("Login", "Account");

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
            if (tourist == null) return RedirectToAction("Login", "Account");

            // Gán TouristID trước khi validate
            feedback.TouristID = tourist.TouristID;
            ModelState.Remove(nameof(Feedback.TouristID));
            ModelState.Remove(nameof(Feedback.Date));

            // Check Booking
            var approvedBooking = await _context.Booking
                .Include(b => b.TourPackage)
                .FirstOrDefaultAsync(b =>
                    b.BookingID == feedback.BookingID &&
                    b.TouristID == tourist.TouristID);

            if (approvedBooking == null)
            {
                ModelState.AddModelError("BookingID", "Booking not found or not yours.");
            }
            else if (!string.Equals(approvedBooking.Status?.Trim(), "APPROVED", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("BookingID", "You can only give feedback for an approved booking.");
            }

            // Debug lỗi ModelState
            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine($"⚠️ Field: {entry.Key}, Error: {error.ErrorMessage}");
                    }
                }

                await PopulateApprovedBookingsAsync(tourist.TouristID, feedback.BookingID);
                return View(feedback);
            }

            feedback.Date = DateTime.UtcNow;
            _context.Add(feedback);
            await _context.SaveChangesAsync();

            TempData["FeedbackSuccess"] = $"Thank you! Feedback for {approvedBooking?.TourPackage?.Title} submitted.";
            return RedirectToAction(nameof(Index));
        }


        private async Task<Tourist?> GetCurrentTouristAsync()
        {
            if (!User.Identity?.IsAuthenticated ?? true) return null;
            if (!User.IsInRole("Tourist")) return null;

            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdValue) || !int.TryParse(userIdValue, out var userId)) return null;

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

        private bool FeedbackExists(int id)
        {
            return _context.Feedbacks.Any(e => e.FeedbackID == id);
        }
    }
}
