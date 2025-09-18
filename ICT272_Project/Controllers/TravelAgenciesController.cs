using ICT272_Project.Data;
using ICT272_Project.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ICT272_Project.Controllers
{
    public class TravelAgenciesController : Controller
    {
        private readonly AppDbContext _context;

        public TravelAgenciesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: TravelAgencies
        public async Task<IActionResult> Index()
        {
            return View(await _context.TravelAgencies.ToListAsync());
        }

        // GET: TravelAgencies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var travelAgency = await _context.TravelAgencies
                .FirstOrDefaultAsync(m => m.AgencyID == id);

            if (travelAgency == null) return NotFound();

            return View(travelAgency);
        }

        // GET: TravelAgencies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var travelAgency = await _context.TravelAgencies.FindAsync(id);
            if (travelAgency == null) return NotFound();

            return View(travelAgency);
        }

        // POST: TravelAgencies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AgencyID,AgencyName,ContactInfo,Description,ServicesOffered,ProfileImage,UserID")] TravelAgency travelAgency)
        {
            if (id != travelAgency.AgencyID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(travelAgency);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TravelAgencyExists(travelAgency.AgencyID))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(travelAgency);
        }

        // GET: TravelAgencies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var agency = await _context.TravelAgencies
                .FirstOrDefaultAsync(m => m.AgencyID == id);

            if (agency == null) return NotFound();

            return View(agency);
        }

        // POST: TravelAgencies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var agency = await _context.TravelAgencies.FindAsync(id);
            if (agency != null)
            {
                var user = await _context.Users.FindAsync(agency.UserID);
                _context.TravelAgencies.Remove(agency);

                if (user != null)
                {
                    _context.Users.Remove(user);
                }

                await _context.SaveChangesAsync();
            }
            await HttpContext.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        private bool TravelAgencyExists(int id)
        {
            return _context.TravelAgencies.Any(e => e.AgencyID == id);
        }
    }
}
