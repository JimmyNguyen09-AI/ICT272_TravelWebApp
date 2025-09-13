using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ICT272_Project.Data;
using ICT272_Project.Models;

namespace ICT272_Project.Controllers
{
    public class TourPackagesController : Controller
    {
        private readonly AppDbContext _context;

        public TourPackagesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: TourPackages
        public async Task<IActionResult> Index()
        {
            return View(await _context.TourPackages.ToListAsync());
        }

        // GET: TourPackages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tourPackage = await _context.TourPackages
                .FirstOrDefaultAsync(m => m.PackageID == id);
            if (tourPackage == null)
            {
                return NotFound();
            }

            return View(tourPackage);
        }

        // GET: TourPackages/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TourPackages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PackageID,AgencyID,Title,Description,Duration,Price,MaxGroupSize")] TourPackage tourPackage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tourPackage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tourPackage);
        }

        // GET: TourPackages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tourPackage = await _context.TourPackages.FindAsync(id);
            if (tourPackage == null)
            {
                return NotFound();
            }
            return View(tourPackage);
        }

        // POST: TourPackages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PackageID,AgencyID,Title,Description,Duration,Price,MaxGroupSize")] TourPackage tourPackage)
        {
            if (id != tourPackage.PackageID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tourPackage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TourPackageExists(tourPackage.PackageID))
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
            return View(tourPackage);
        }

        // GET: TourPackages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tourPackage = await _context.TourPackages
                .FirstOrDefaultAsync(m => m.PackageID == id);
            if (tourPackage == null)
            {
                return NotFound();
            }

            return View(tourPackage);
        }

        // POST: TourPackages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tourPackage = await _context.TourPackages.FindAsync(id);
            if (tourPackage != null)
            {
                _context.TourPackages.Remove(tourPackage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TourPackageExists(int id)
        {
            return _context.TourPackages.Any(e => e.PackageID == id);
        }
    }
}
