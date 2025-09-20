using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ICT272_Project.Data;
using ICT272_Project.Models;
using System.Security.Claims;

namespace ICT272_Project.Controllers
{
    public class BookingsController : Controller
    {
        private readonly AppDbContext _context;

        public BookingsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var bookingsQuery = _context.Booking
                .Include(b => b.Tourist)
                .Include(b => b.TourPackage)
                    .ThenInclude(tp => tp.TravelAgency)
                .AsQueryable();

            if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out var parsedUserId))
            {
                if (User.IsInRole("Tourist"))
                {
                    bookingsQuery = bookingsQuery.Where(b => b.Tourist.UserID == parsedUserId);
                }
                else if (User.IsInRole("Agency"))
                {
                    bookingsQuery = bookingsQuery.Where(b => b.TourPackage.TravelAgency != null &&
                                                             b.TourPackage.TravelAgency.UserID == parsedUserId);
                }
            }

            var bookings = await bookingsQuery.ToListAsync();
            return View(bookings);
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Booking
                .Include(b => b.Tourist)
                .Include(b => b.TourPackage)
                .FirstOrDefaultAsync(m => m.BookingID == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            ViewBag.TourPackages = new SelectList(_context.TourPackages, "PackageID", "Title");
            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PackageID,BookingDate,NumberofPaticipants")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var parsedUserId))
                {
                    ModelState.AddModelError("", "User is not logged in.");
                    return View(booking);
                }
                var tourist = await _context.Tourists.FirstOrDefaultAsync(t => t.UserID == parsedUserId);
                if (tourist == null)
                {
                    ModelState.AddModelError("", "Tourist account not found.");
                    return View(booking);
                }

                booking.TouristID = tourist.TouristID;

                var tourPackage = await _context.TourPackages.FindAsync(booking.PackageID);
                if (tourPackage == null)
                {
                    ModelState.AddModelError("PackageID", "Invalid tour package selected");
                    return View(booking);
                }

                if (booking.NumberofPaticipants > tourPackage.MaxGroupSize)
                {
                    ModelState.AddModelError("NumberofPaticipants", $"Group size cannot exceed {tourPackage.MaxGroupSize}");
                    return View(booking);
                }

                booking.Status = "Pending";

                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.TourPackages = new SelectList(_context.TourPackages, "PackageID", "Title", booking.PackageID);
            return View(booking);
        }


        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (id != booking.BookingID) return NotFound();

            var existingBooking = await _context.Booking.AsNoTracking().FirstOrDefaultAsync(b => b.BookingID == id);
            if (existingBooking == null) return NotFound();

            booking.TouristID = existingBooking.TouristID;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingID)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                TempData["StatusError"] = "Invalid status value provided.";
                return RedirectToAction(nameof(Index));
            }

            var booking = await _context.Booking
                .Include(b => b.Tourist)
                .Include(b => b.TourPackage)
                    .ThenInclude(tp => tp.TravelAgency)
                .FirstOrDefaultAsync(b => b.BookingID == id);

            if (booking == null)
            {
                TempData["StatusError"] = "Booking not found.";
                return RedirectToAction(nameof(Index));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Agency") ||
                string.IsNullOrEmpty(userId) ||
                booking.TourPackage?.TravelAgency == null ||
                booking.TourPackage.TravelAgency.UserID.ToString() != userId)
            {
                TempData["StatusError"] = "You are not authorized to update this booking.";
                return RedirectToAction(nameof(Index));
            }

            var normalizedStatus = status.Trim();
            if (normalizedStatus.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            {
                booking.Status = "Approved";
            }
            else if (normalizedStatus.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            {
                booking.Status = "Rejected";
            }
            else if (normalizedStatus.Equals("Pending", StringComparison.OrdinalIgnoreCase))
            {
                booking.Status = "Pending";
            }
            else
            {
                TempData["StatusError"] = "Unsupported booking status.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Update(booking);
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = $"Booking for {booking.Tourist?.FullName ?? "the selected tourist"} marked as {booking.Status}.";
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["StatusError"] = "Unable to update booking status at this time.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Booking
                .Include(b => b.Tourist)
                .Include(b => b.TourPackage)
                .FirstOrDefaultAsync(m => m.BookingID == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking != null)
            {
                _context.Booking.Remove(booking);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Booking.Any(e => e.BookingID == id);
        }
    }
}
