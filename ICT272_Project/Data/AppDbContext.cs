using Microsoft.EntityFrameworkCore;
using ICT272_Project.Models;

namespace ICT272_Project.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Tourist> Tourists => Set<Tourist>();
        public DbSet<Feedback> Feedbacks => Set<Feedback>();

        public DbSet<TourPackage> TourPackages => Set<TourPackage>();
        public DbSet<TravelAgency> TravelAgencies => Set<TravelAgency>();

        public DbSet<User> Users => Set<User>();
    }
}
