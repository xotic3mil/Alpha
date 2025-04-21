using System.ComponentModel.DataAnnotations;

namespace Business.Dtos;

public class UserRegForm
{
    [Required(ErrorMessage = "Required")]
    [Display(Name = "First Name", Prompt = "Enter your First name")]
    [DataType(DataType.Text)]
    public  string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Required")]
    [Display(Name = "Last Name", Prompt = "Enter your Last name")]
    [DataType(DataType.Text)]
    public  string LastName { get; set; } = null!;

    [Required(ErrorMessage = "Required")]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format")]
    [Display(Name = "Email", Prompt = "Enter your Email")]
    [DataType(DataType.EmailAddress)]
    public  string Email { get; set; } = null!;

    [Required]
    [Display(Name = "Phone", Prompt = "Enter your Phone number")]
    [DataType(DataType.PhoneNumber)]
    public string PhoneNumber { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public string? Title { get; set; }

    [Required(ErrorMessage = "Required")]
    [Display(Name = "Password", Prompt = "Enter your Password")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Required")]
    [Display(Name = "Confirm Password", Prompt = "Confirm your password")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = null!;

    [Required(ErrorMessage = "You must accept the Terms and Conditions.")]
    [Display(Name = "Terms & Conditions", Prompt = "I accept the terms & conditions")]
    public bool TermsAndCondition { get; set; } 
}

