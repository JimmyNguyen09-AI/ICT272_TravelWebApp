using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ICT272_Project.Models
{
    public class TourPackage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PackageID { get; set; }
        [ForeignKey("TravelAgency")]
        public int AgencyID { get; set; }
        [ValidateNever]
        public TravelAgency TravelAgency { get; set; }
        [Required(ErrorMessage ="Title is required"), StringLength(150)]
        public string Title { get; set; }
        [Required(ErrorMessage ="Description is required")]
        public string Description { get; set; }
        [Required(ErrorMessage ="Duration must be between 1 and 365 days")]
        public int Duration { get; set; }
        [Required(ErrorMessage ="Price is required"), Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }
        [Required(ErrorMessage ="Group size must be between 1 and 50")]
        public int MaxGroupSize { get; set; }

    }
}
