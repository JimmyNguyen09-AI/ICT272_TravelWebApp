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
            var appDbContext = _context.Booking
                .Include(b => b.Tourist)
                .Include(b => b.TourPackage);
            return View(await appDbContext.ToListAsync());
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var tourist = await _context.Tourists
                .FirstOrDefaultAsync(t => t.UserID.ToString() == userId);

            if (tourist == null)
            {
                ModelState.AddModelError("", "No tourist profile linked to this account.");
                ViewBag.TourPackages = new SelectList(_context.TourPackages, "PackageID", "Title", booking.PackageID);
                return View(booking);
            }

            booking.TouristID = tourist.TouristID;

            if (ModelState.IsValid)
            {
                var tourPackage = await _context.TourPackages.FindAsync(booking.PackageID);
                if (tourPackage == null)
                {
                    ModelState.AddModelError("PackageID", "Invalid tour package selected");
                }
                else if (booking.NumberofPaticipants > tourPackage.MaxGroupSize)
                {
                    ModelState.AddModelError("NumberofPaticipants", $"Group size cannot exceed the tour limit ({tourPackage.MaxGroupSize})");
                }
                else
                {
                    booking.Status = "Pending";
                    _context.Add(booking);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewBag.TourPackages = new SelectList(_context.TourPackages, "PackageID", "Title", booking.PackageID);
            return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Booking
                .Include(b => b.Tourist)
                .FirstOrDefaultAsync(b => b.BookingID == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingID,PackageID,BookingDate,Status,NumberofPaticipants")] Booking booking)
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
