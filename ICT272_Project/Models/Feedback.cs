using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ICT272_Project.Models
{
    public class Feedback
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FeedbackID { get; set; }

        [Required]
        public int BookingID { get; set; }
        [Required]
        [ForeignKey("TouristID")]
        public int TouristID { get; set; }
        [ValidateNever]
        public Tourist Tourist { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }

}
