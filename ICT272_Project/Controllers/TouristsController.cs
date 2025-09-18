using ICT272_Project.Data;
using ICT272_Project.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ICT272_Project.Controllers
{
    public class TouristsController : Controller
    {
        private readonly AppDbContext _context;

        public TouristsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Tourists
        public async Task<IActionResult> Index()
        {
            return View(await _context.Tourists.ToListAsync());
        }

        // GET: Tourists/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var tourist = await _context.Tourists
                .FirstOrDefaultAsync(m => m.TouristID == id);

            if (tourist == null) return NotFound();

            return View(tourist);
        }

        // GET: Tourists/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var tourist = await _context.Tourists.FindAsync(id);
            if (tourist == null) return NotFound();

            return View(tourist);
        }

        // POST: Tourists/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TouristID,FullName,Email,ContactNumber,UserID")] Tourist tourist)
        {
            if (id != tourist.TouristID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Tourists.AsNoTracking()
                        .FirstOrDefaultAsync(t => t.TouristID == id);
                    if (existing == null) return NotFound();

                    tourist.PasswordHash = existing.PasswordHash;
                    _context.Update(tourist);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TouristExists(tourist.TouristID))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tourist);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var tourist = await _context.Tourists
                .FirstOrDefaultAsync(m => m.TouristID == id);

            if (tourist == null) return NotFound();

            return View(tourist);
        }

        // POST: Tourists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tourist = await _context.Tourists.FindAsync(id);
            if (tourist != null)
            {
                var user = await _context.Users.FindAsync(tourist.UserID);
                _context.Tourists.Remove(tourist);

                if (user != null)
                {
                    _context.Users.Remove(user);
                }

                await _context.SaveChangesAsync();
            }
            await HttpContext.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        private bool TouristExists(int id)
        {
            return _context.Tourists.Any(e => e.TouristID == id);
        }
    }
}
