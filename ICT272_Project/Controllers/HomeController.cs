using ICT272_Project.Data;
using ICT272_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ICT272_Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var tours = await _context.TourPackages
                .Include(t => t.TravelAgency)
                .Take(3)
                .ToListAsync();

            var agencies = await _context.TravelAgencies
                .Take(3)
                .ToListAsync();

            var feedbacks = await _context.Feedbacks
                .Include(f => f.Tourist)
                .Take(3)
                .ToListAsync();

            ViewBag.Tours = tours;
            ViewBag.Agencies = agencies;
            ViewBag.Feedbacks = feedbacks;

            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
