using System.ComponentModel.DataAnnotations;

namespace Business.Dtos;

public class UserLoginForm
{
    [Required(ErrorMessage = "Required")]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format")]
    [Display(Name = "Email", Prompt = "Enter your Email")]
    [DataType(DataType.EmailAddress)]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Required")]
    [Display(Name = "Password", Prompt = "Enter your Password")]
    [DataType(DataType.Password)]
    public required string Password { get; set; }

    public bool RememberMe { get; set; }
}
