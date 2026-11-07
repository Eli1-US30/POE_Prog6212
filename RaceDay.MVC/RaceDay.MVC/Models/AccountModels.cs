using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models
{
    public class RegisterViewModel
    {
        [Required, StringLength(100), Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required, Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password), Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Participant";
    }

    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    // Shape of the API's login response
    public class LoginResult
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}