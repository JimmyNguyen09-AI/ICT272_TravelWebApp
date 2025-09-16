using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ICT272_Project.Models
{
    public class Booking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BookingID { get; set; }
        [Required]
        public int TouristID { get; set; }
        [ValidateNever]
        public Tourist Tourist { get; set; }
        [Required]
        public int PackageID { get; set; }
        [ValidateNever]
        public TourPackage TourPackage { get; set; }
        [Required]
        public DateTime BookingDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string Status { get; set; } = "Pending";
        [Required]
        [Range(1,500,ErrorMessage = "Group size is invalid")]
        public int NumberofPaticipants { get; set; }
    }
}
