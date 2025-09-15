using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ICT272_Project.Models
{
    [Authorize(Roles = "Agency")]
    public class TravelAgency
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AgencyID { get; set; }
        [Required(ErrorMessage = "Please enter Agency Name"),StringLength(100)]
        public string AgencyName { get; set; }
        [Required(ErrorMessage ="Please provide contact information"),StringLength(200)]
        public string ContactInfo { get; set; }
        public string Description { get; set; }
        [Required,StringLength(200, ErrorMessage ="Services offered text too long")]
        public string ServicesOffered { get; set; }
        public string ProfileImage { get; set; }

        public ICollection<TourPackage> TourPackages { get; set; } = new List<TourPackage>();

         
    }
}
