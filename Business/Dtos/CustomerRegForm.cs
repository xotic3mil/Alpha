using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Dtos;

public class CustomerRegForm
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Company name is required")]
    public string CompanyName { get; set; } = null!;

    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Please enter a valid phone number")]
    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }

    public string? ContactName { get; set; }
}
