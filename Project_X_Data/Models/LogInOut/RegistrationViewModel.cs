using System.ComponentModel.DataAnnotations;

namespace Project_X_Data.Models.LogInOut
{
    public class RegistrationViewModel
    {
        [Required]
        [Display(Name = "Email or phone number")]
        public String Email { get; set; } = null!;

        [Required]
        [MinLength(6)]
        [DataType(DataType.Password)]
        public String Password { get; set; } = null!;

        [Compare("Password")]
        [DataType(DataType.Password)]
        public String ConfirmPassword { get; set; } = null!;
    }
}
