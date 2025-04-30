namespace Domain.Models;

public class Customer
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = null!;

    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string City { get; set; } = null!;

    public string Country { get; set; } = null!;

    public string ContactName { get; set; } = null!;


}
