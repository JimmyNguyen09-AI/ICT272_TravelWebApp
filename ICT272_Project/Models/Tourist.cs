using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ICT272_Project.Models
{
    public class Tourist
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TouristID { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(20)]
        public string? ContactNumber { get; set; }
        [ForeignKey("User")]
        public int UserID { get; set; }

        [ValidateNever]
        public User User { get; set; }

        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    }

}
