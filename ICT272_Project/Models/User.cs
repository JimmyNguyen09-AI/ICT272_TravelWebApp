using System.ComponentModel.DataAnnotations;

namespace ICT272_Project.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required(ErrorMessage ="Username is required")]
        [StringLength(50)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage ="Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage ="Password is required")]
        [StringLength(255, MinimumLength =6, ErrorMessage ="Password must be at least 6 Characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Role is Required")]
        [RegularExpression("Tourist|Agency")]
        public string Role { get; set; }
    }
}
