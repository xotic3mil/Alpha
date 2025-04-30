using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

[Table("Customers")]
public class CustomerEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = null!;

    public string Email { get; set; } = null!;

    [Phone(ErrorMessage = "Please enter a valid phone number")]
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }

    public string? ContactName { get; set; }

    public ICollection<ProjectEntity>? Projects { get; set; } = [];

}
